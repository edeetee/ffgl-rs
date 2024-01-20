use std::{fmt::Formatter, rc::Rc};

pub use ffgl_raw::*;
// use egui_node_graph::graph;
// mod ffgl;
// use ::ffgl::{ffgl_handler, FFGLHandler};
use glium::{
    backend::Context,
    framebuffer::{RenderBuffer, SimpleFrameBuffer},
    BlitTarget, Frame, Surface,
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
    fn info() -> &'static mut ffi::ffgl1::PluginInfoStruct;
    fn new(inst_data: &FFGLData, ctx: Rc<Context>) -> Self;
    fn render_frame(&mut self, inst_data: &FFGLData, target: &mut impl Surface);
}

impl<Handler: ParamHandler + Debug> ParamHandler for FFGLGlium<Handler> {
    type Param = Handler::Param;

    fn num_params() -> usize {
        Handler::num_params()
    }

    fn param(index: usize) -> &'static Self::Param {
        Handler::param(index)
    }

    fn set_param(&mut self, index: usize, value: parameters::ParamValue) {
        self.handler.set_param(index, value);
    }
}

impl<Handler: FFGLGliumHandler + Debug> FFGLHandler for FFGLGlium<Handler> {
    unsafe fn info() -> &'static mut ffi::ffgl1::PluginInfoStruct {
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

    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: &ffi::ffgl1::ProcessOpenGLStruct) {
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

        self.handler.render_frame(inst_data, fb);

        // validate_viewport(&viewport);

        //puts the texture into the framebuffer
        fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // gl::BindFramebuffer(gl::READ_FRAMEBUFFER, 0);
        gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.HostFBO);
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
