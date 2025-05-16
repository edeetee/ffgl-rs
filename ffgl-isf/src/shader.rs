use std::{str::FromStr, time::Instant};

use ffgl_glium::glsl::get_best_transpilation_target;
use glium::{
    backend::Facade,
    texture::{MipmapsOption, UncompressedFloatFormat},
    uniforms::{AsUniformValue, UniformValue, Uniforms},
    DrawError, Surface, Texture2d,
};
use isf::{Isf, Pass};
use tracing::debug;

use crate::{fullscreen_shader::FullscreenFrag, util::GlProgramCreationError};
use thiserror::Error;

use build_common::isf_glsl_preprocess::compile_isf_fragment;

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

fn texture_from_pass(
    facade: &impl Facade,
    pass: &Pass,
    res: (u32, u32),
) -> Result<Texture2d, PassParseError> {
    let size = calculate_pass_size(pass, res);

    let texture = Texture2d::empty_with_format(
        facade,
        if pass.float {
            UncompressedFloatFormat::F32F32F32F32
        } else {
            UncompressedFloatFormat::U8U8U8U8
        },
        MipmapsOption::NoMipmap,
        size.0,
        size.1,
    )
    .expect("Failed to create texture");

    Ok(texture)
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
        let texture = texture_from_pass(facade, &pass, res).expect("Failed to create texture");

        Ok(Self { pass, texture })
    }

    pub fn update_size(&mut self, facade: &impl Facade, size: (u32, u32)) {
        let new_texture =
            texture_from_pass(facade, &self.pass, size).expect("Failed to create texture");

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
        let glsl_version_target = get_best_transpilation_target(facade)
            .ok_or(IsfShaderLoadError::NoSupportedGlslVersion)?;

        debug!("GLSL VERSION TARGET: {:?}", glsl_version_target);

        let source = compile_isf_fragment(&isf, &original_source, glsl_version_target);

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
            for pass_tex in &self.passes {
                if pass_tex.pass.target.is_none() {
                    self.frag.draw(surface, &uniforms)?;
                } else {
                    self.frag
                        .draw(&mut pass_tex.texture.as_surface(), &uniforms)?;
                }
                uniforms.pass_index += 1;
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
    #[error("Load error")]
    IoError(#[from] std::io::Error),

    #[error("Compile error")]
    CompileError(#[from] GlProgramCreationError),

    #[error("Parse error")]
    PassParseError(#[from] PassParseError),

    #[error("No supported GLSL version error")]
    NoSupportedGlslVersion,
}
