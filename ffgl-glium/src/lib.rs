use std::{error::Error, fmt::Formatter, rc::Rc};

pub use ffgl_core::*;
// use egui_node_graph::graph;
// mod ffgl;
// use ::ffgl::{ffgl_handler, FFGLHandler};
use glium::{
    backend::Context,
    framebuffer::{RenderBuffer, SimpleFrameBuffer},
    BlitTarget, Frame, Surface, Texture2d,
};
use std::fmt::Debug;

mod gl_backend;
pub mod texture;
pub mod validate_gl;

pub struct FFGLGliumInstance {
    pub ctx: Rc<Context>,
    backend: Rc<gl_backend::RawGlBackend>,
}

impl Debug for FFGLGliumInstance {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("FFGLGliumHandler").finish()
    }
}

impl FFGLGliumInstance {
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

        tracing::debug!("OPENGL_VERSION {}", ctx.get_opengl_version_string());

        Self { ctx, backend }
    }

    pub fn draw(
        &self,
        inst_data: &FFGLData,
        frame_data: GLInput<'_>,
        render_frame: &mut impl FnMut(
            &mut SimpleFrameBuffer,
            Vec<Texture2d>,
        ) -> Result<(), Box<dyn Error>>,
    ) {
        let res = inst_data.get_dimensions();

        unsafe { self.ctx.rebuild(self.backend.clone()).unwrap() };

        let frame = Frame::new(self.ctx.clone(), (res.0, res.1));
        let rb = RenderBuffer::new(
            &self.ctx,
            glium::texture::UncompressedFloatFormat::U8U8U8U8,
            res.0,
            res.1,
        )
        .unwrap();

        let fb = &mut SimpleFrameBuffer::new(&self.ctx, &rb).unwrap();
        // fb.clear_color(0.0, 0.0, 1.0, 1.0);

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
            tracing::error!("ERROR: {err}");
        }

        // validate_viewport(&viewport);

        //puts the texture into the framebuffer
        fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // gl::BindFramebuffer(gl::READ_FRAMEBUFFER, 0);
        unsafe {
            gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);
            blit_fb(res, res);
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
