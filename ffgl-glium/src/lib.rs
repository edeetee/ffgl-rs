//! Utilities for creating FFGL plugins using the glium library.
//!
//! Use [FFGLGlium] in your plugin to render frames with a glium context.
//!
//! Just call [FFGLGlium::draw] inside your [ffgl_core::handler::FFGLInstance::draw] method.
//!
//! See example-isf for a good example
//!
//!### !WARNING!
//!
//! I make assumptions about the OpenGL context inside the host. Bugs and crashes may occur. Testing infrastructure is required.
//!
use std::{error::Error, fmt::Formatter, rc::Rc};

use ffgl_core::*;
use glium::{
    backend::Context,
    framebuffer::{RenderBuffer, SimpleFrameBuffer},
    BlitTarget, CapabilitiesSource, Frame, Surface, Texture2d,
};
use std::fmt::Debug;

mod gl_backend;
pub mod glsl;
pub mod texture;
pub mod validate_gl;

///Use this struct to render frames with a glium context, making assumptions about the OpenGL context inside an FFGL host.
pub struct FFGLGlium {
    pub ctx: Rc<Context>,
    backend: Rc<gl_backend::RawGlBackend>,
}

impl Debug for FFGLGlium {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("FFGLGliumHandler").finish()
    }
}

impl FFGLGlium {
    pub fn new(inst_data: &FFGLData) -> Self {
        let backend = Rc::new(gl_backend::RawGlBackend::new(inst_data.get_dimensions()));

        tracing::debug!("BACKEND: {backend:?}");

        let ctx = unsafe {
            glium::backend::Context::new(
                backend.clone(),
                false,
                glium::debug::DebugCallbackBehavior::Ignore,
            )
            .unwrap()
        };

        let valid_versions = &ctx.get_capabilities().supported_glsl_versions;

        tracing::debug!("VALID VERSIONS: {valid_versions:?}");

        tracing::debug!("OPENGL_VERSION {}", ctx.get_opengl_version_string());

        Self { ctx, backend }
    }

    pub fn draw(
        &self,
        output_res: (u32, u32),
        render_res: (u32, u32),
        frame_data: GLInput<'_>,
        render_frame: &mut impl FnMut(
            &mut SimpleFrameBuffer,
            Vec<Texture2d>,
        ) -> Result<(), Box<dyn Error>>,
    ) {
        unsafe { self.ctx.rebuild(self.backend.clone()).unwrap() };

        let frame = Frame::new(self.ctx.clone(), (render_res.0, render_res.1));
        let rb = RenderBuffer::new(
            &self.ctx,
            glium::texture::UncompressedFloatFormat::U8U8U8U8,
            render_res.0,
            render_res.1,
        )
        .unwrap();

        let fb = &mut SimpleFrameBuffer::new(&self.ctx, &rb).unwrap();

        let textures: Vec<_> = frame_data
            .textures
            .iter()
            .map(|texture_info| unsafe {
                Texture2d::from_id(
                    &self.ctx,
                    glium::texture::UncompressedFloatFormat::U8U8U8U8,
                    texture_info.Handle,
                    false,
                    glium::texture::MipmapsOption::NoMipmap,
                    glium::texture::Dimensions::Texture2d {
                        width: texture_info.Width,
                        height: texture_info.Height,
                    },
                )
            })
            .collect();

        if let Err(err) = render_frame(fb, textures) {
            tracing::error!("Render ERROR: {err:?}");
        }

        // validate_viewport(&viewport);

        //puts the texture into the framebuffer
        fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // gl::BindFramebuffer(gl::READ_FRAMEBUFFER, 0);
        unsafe {
            gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);
            blit_fb(render_res, output_res);
        }

        frame.finish().unwrap();

        //reset to what host expects
        // gl_reset(frame_data);
        // validate::validate_context_state();

        // validate_viewport(&viewport);
    }
}

unsafe fn blit_fb((read_w, read_h): (u32, u32), (write_w, write_h): (u32, u32)) {
    let src_rect = BlitTarget {
        left: 0,
        bottom: 0,
        width: read_w as i32,
        height: read_h as i32,
    };

    let target_rect = BlitTarget {
        left: 0 as u32,
        bottom: 0 as u32,
        width: write_w as i32,
        height: write_h as i32,
    };

    gl::BlitFramebuffer(
        src_rect.left as gl::types::GLint,
        src_rect.bottom as gl::types::GLint,
        (src_rect.left as i32 + src_rect.width) as gl::types::GLint,
        (src_rect.bottom as i32 + src_rect.height) as gl::types::GLint,
        (target_rect.left) as gl::types::GLint,
        (target_rect.bottom) as gl::types::GLint,
        (target_rect.left as i32 + target_rect.width) as gl::types::GLint,
        (target_rect.bottom as i32 + target_rect.height) as gl::types::GLint,
        gl::COLOR_BUFFER_BIT,
        gl::NEAREST,
    );
}
