use build_common::transform_glsl;
use ffgl_glium::glsl::get_best_transpilation_target;
use glium::{
    backend::Facade, implement_vertex, index, uniforms::Uniforms, Blend, DrawError, DrawParameters,
    Program, ProgramCreationError, Smooth, Surface, VertexBuffer,
};

use crate::util::{GlProgramCreationError, MultiUniforms};
#[derive(Debug)]
pub struct FullscreenFrag {
    pub verts: VertexBuffer<VertexAttr>,
    pub program: Program,
    pub params: DrawParameters<'static>,
}

impl FullscreenFrag {
    pub fn new(facade: &impl Facade, frag: &str) -> Result<Self, GlProgramCreationError> {
        let params = DrawParameters {
            dithering: true,
            smooth: Some(Smooth::Fastest),
            blend: Blend::alpha_blending(),
            ..Default::default()
        };

        Self::new_with_params(facade, frag, params)
    }

    pub fn new_with_params(
        facade: &impl Facade,
        frag: &str,
        params: DrawParameters<'static>,
    ) -> Result<Self, GlProgramCreationError> {
        let vert_buffer = new_fullscreen_buffer(facade).unwrap();

        let transpilation_target = get_best_transpilation_target(facade).ok_or_else(|| {
            GlProgramCreationError::new(
                ProgramCreationError::CompilationNotSupported,
                frag.to_string(),
                None,
            )
        })?;

        let program = Program::from_source(
            facade,
            &transform_glsl(FULLSCREEN_VERT_SHADER, transpilation_target),
            frag,
            None,
        )
        .map_err(|e| {
            GlProgramCreationError::new(e, frag.to_string(), Some(transpilation_target))
        })?;

        Ok(Self {
            params,
            verts: vert_buffer,
            program,
        })
    }

    pub fn draw(
        &self,
        surface: &mut impl Surface,
        uniforms: &impl Uniforms,
    ) -> Result<(), DrawError> {
        let dim = surface.get_dimensions();

        surface.draw(
            &self.verts,
            &FULLSCREEN_INDICES,
            &self.program,
            &MultiUniforms::single("res", &[dim.0 as f32, dim.1 as f32], uniforms),
            &self.params,
        )
    }
}

#[derive(Copy, Clone, Debug)]
pub struct VertexAttr {
    position: [f32; 3],
}
implement_vertex!(VertexAttr, position);

const fn v(x: f32, y: f32, z: f32) -> VertexAttr {
    VertexAttr {
        position: [x, y, z],
    }
}

const FULLSCREEN_VERTICES: [VertexAttr; 3] =
    [v(-1.0, -1.0, 0.0), v(3.0, -1.0, 0.0), v(-1.0, 3.0, 0.0)];

pub const FULLSCREEN_INDICES: glium::index::NoIndices =
    index::NoIndices(index::PrimitiveType::TrianglesList);

pub fn new_fullscreen_buffer<F: ?Sized + Facade>(
    facade: &F,
) -> Result<VertexBuffer<VertexAttr>, glium::vertex::BufferCreationError> {
    VertexBuffer::immutable(facade, &FULLSCREEN_VERTICES)
}

pub const FULLSCREEN_VERT_SHADER: &'static str = include_str!("fullscreen.vert");
