use std::{error::Error, fmt::Formatter, rc::Rc};

use ffgl_raw::ffi::ffgl2::{self, FFGLTextureStruct};
pub use ffgl_raw::*;
// use egui_node_graph::graph;
// mod ffgl;
// use ::ffgl::{ffgl_handler, FFGLHandler};
use glium::{
    backend::{Context, Facade},
    framebuffer::{RenderBuffer, SimpleFrameBuffer},
    BlitTarget, Frame, Surface, Texture2d,
};
use std::fmt::Debug;

mod gl_backend;
mod validate_gl;

pub struct FFGLGlium<Handler: Debug> {
    ctx: Rc<Context>,
    backend: Rc<gl_backend::RawGlBackend>,
    handler: Handler,
}

impl<Handler: Debug> Debug for FFGLGlium<Handler> {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("FFGLGliumHandler")
            .field("handler", &self.handler)
            .finish()
    }
}

pub trait FFGLGliumHandler: Sized + ParamHandler {
    fn info() -> &'static ffi::ffgl2::PluginInfoStruct;
    fn new(inst_data: &FFGLData, ctx: Rc<Context>) -> Self;
    fn render_frame(
        &mut self,
        target: &mut impl Surface,
        input_textures: Vec<Texture2d>,
        inst_data: &FFGLData,
    ) -> Result<(), Box<dyn Error>>;
}

impl<Handler: ParamHandler + Debug> ParamHandler for FFGLGlium<Handler> {
    type Param = Handler::Param;

    fn num_params() -> usize {
        Handler::num_params()
    }

    fn param_info(index: usize) -> &'static Self::Param {
        Handler::param_info(index)
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.handler.set_param(index, value);
    }

    fn get_param(&self, index: usize) -> f32 {
        self.handler.get_param(index)
    }
}

impl<Handler: FFGLGliumHandler + Debug> FFGLHandler for FFGLGlium<Handler> {
    unsafe fn info() -> &'static ffi::ffgl2::PluginInfoStruct {
        Handler::info()
    }

    unsafe fn new(inst_data: &FFGLData) -> Self {
        let backend = Rc::new(gl_backend::RawGlBackend::new(inst_data.get_dimensions()));

        logln!("BACKEND: {backend:?}");

        let ctx = glium::backend::Context::new(
            backend.clone(),
            false,
            glium::debug::DebugCallbackBehavior::Ignore,
        )
        .unwrap();

        logln!("OPENGL_VERSION {}", ctx.get_opengl_version_string());

        Self {
            handler: Handler::new(inst_data, ctx.clone()),
            ctx,
            backend,
        }
    }

    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput<'_>) {
        let res = inst_data.get_dimensions();
        self.ctx.rebuild(self.backend.clone()).unwrap();

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

        if let Err(err) = self.handler.render_frame(fb, textures, inst_data) {
            logln!("ERROR: {err}");
        }

        // validate_viewport(&viewport);

        //puts the texture into the framebuffer
        fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // gl::BindFramebuffer(gl::READ_FRAMEBUFFER, 0);
        gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);
        blit_fb(res, res);

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
