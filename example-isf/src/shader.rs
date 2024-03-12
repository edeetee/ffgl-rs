use std::{str::FromStr, time::Instant};

use ffgl_glium::texture::{new_texture_2d, DEFAULT_RES};
use glium::{
    backend::Facade,
    uniforms::{AsUniformValue, UniformValue, Uniforms},
    DrawError, Surface, Texture2d,
};
use isf::{Isf, Pass};

use crate::{fullscreen_shader::FullscreenFrag, util::GlProgramCreationError};
use thiserror::Error;

use build_common::isf_glsl_preprocess::convert_fragment_source_to_glsl_120;

pub struct IsfShader {
    frag: FullscreenFrag,
    passes: Vec<PassTexture>,
    start_inst: Instant,
    prev_frame_inst: Instant,
    frame_count: u32,
}

struct PassTexture {
    pass: Pass,
    texture: Texture2d,
}

impl Uniforms for PassTexture {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut f: F) {
        if let Some(name) = self.pass.target.as_ref() {
            f(name, self.texture.as_uniform_value());
        }
    }
}

#[derive(Error, Debug)]
#[error("Could not parse pass size {0}")]
pub struct PassParseError(String);

fn parse_or_default<T: FromStr>(text: &Option<String>, default: T) -> T {
    text.as_ref()
        .map(|t| t.parse().ok())
        .flatten()
        .unwrap_or(default)
}

fn calculate_pass_size(pass: &Pass, (width, height): (u32, u32)) -> (u32, u32) {
    (
        parse_or_default(&pass.width, width),
        parse_or_default(&pass.height, height),
    )
}

impl PassTexture {
    fn new(
        facade: &impl Facade,
        pass: Pass,
        res: (u32, u32),
    ) -> Result<PassTexture, PassParseError> {
        let size = calculate_pass_size(&pass, res);

        Ok(Self {
            pass,
            texture: new_texture_2d(facade, size).unwrap(),
        })
    }

    pub fn update_size(&mut self, facade: &impl Facade, size: (u32, u32)) {
        let size = calculate_pass_size(&self.pass, size);
        let new_texture = new_texture_2d(facade, size).unwrap();
        self.texture.as_surface().fill(
            &new_texture.as_surface(),
            glium::uniforms::MagnifySamplerFilter::Nearest,
        );

        self.texture = new_texture;
    }
}

impl IsfShader {
    pub fn new(
        facade: &impl Facade,
        isf: &Isf,
        dimensions: (u32, u32),
        original_source: &str,
    ) -> Result<Self, IsfShaderLoadError> {
        let source = convert_fragment_source_to_glsl_120(&isf, &original_source);

        let passes = isf
            .passes
            .iter()
            .map(|pass| PassTexture::new(facade, pass.clone(), dimensions))
            .collect::<Result<_, _>>()?;

        let now = Instant::now();

        Ok(Self {
            frag: FullscreenFrag::new(facade, &source)?,
            start_inst: now,
            prev_frame_inst: now,
            frame_count: 0,
            passes,
            // res
        })
    }

    pub fn try_update_size(&mut self, facade: &impl Facade, size: (u32, u32)) {
        for pass in &mut self.passes {
            if pass.texture.dimensions() != size {
                pass.update_size(facade, size)
            }
        }
    }

    pub fn draw(
        &mut self,
        surface: &mut impl Surface,
        uniforms: &impl Uniforms,
    ) -> Result<(), DrawError> {
        let now = Instant::now();
        let time_delta = now - self.prev_frame_inst;
        let time_total = now - self.start_inst;

        let mut uniforms = IsfUniforms {
            inner: uniforms,
            time_delta: time_delta.as_secs_f32(),
            time: time_total.as_secs_f32(),
            frame_index: self.frame_count,
            pass_index: 0,
            passes: &self.passes,
        };

        if self.passes.is_empty() {
            self.frag.draw(surface, &uniforms)?;
        } else {
            let filter = glium::uniforms::MagnifySamplerFilter::Nearest;

            for pass_tex in &self.passes {
                uniforms.pass_index += 1;
                self.frag.draw(surface, &uniforms)?;
                surface.fill(&pass_tex.texture.as_surface(), filter);
            }
        }

        self.frame_count += 1;

        Ok(())
    }
}

struct IsfUniforms<'a, U: Uniforms> {
    frame_index: u32,
    time_delta: f32,
    time: f32,
    pass_index: i32,
    passes: &'a Vec<PassTexture>,
    inner: &'a U,
}

impl<U: Uniforms> Uniforms for IsfUniforms<'_, U> {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut f: F) {
        f(
            "FRAMEINDEX",
            UniformValue::SignedInt(self.frame_index as i32),
        );
        f("TIMEDELTA", self.time_delta.as_uniform_value());
        f("TIME", self.time.as_uniform_value());
        f("PASSINDEX", self.pass_index.as_uniform_value());
        for PassTexture { pass, texture } in self.passes {
            if let Some(name) = pass.target.as_ref() {
                f(name, texture.as_uniform_value());
                f(
                    &format!("{name}_size"),
                    UniformValue::Vec2([
                        texture.dimensions().0 as f32,
                        texture.dimensions().1 as f32,
                    ]),
                )
            }
        }
        self.inner.visit_values(f);
    }
}

#[derive(Error, Debug)]
pub enum IsfShaderLoadError {
    #[error("Load error {0}")]
    IoError(#[from] std::io::Error),

    #[error("Compile error {0}")]
    CompileError(#[from] GlProgramCreationError),

    #[error("Parse error {0}")]
    PassParseError(#[from] PassParseError),
}
