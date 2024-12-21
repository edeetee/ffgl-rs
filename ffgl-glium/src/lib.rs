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
    debug,
    framebuffer::{
        DefaultFramebuffer, EmptyFrameBuffer, MultiOutputFrameBuffer, RenderBuffer,
        SimpleFrameBuffer,
    },
    BlitTarget, CapabilitiesSource, Frame, GlObject, Surface, Texture2d,
};
use std::fmt::Debug;
use tracing::{debug, trace};

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

pub type DefaultSurface<'a> = Frame;

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
        render_res: (u32, u32),
        out_res: (u32, u32),
        frame_data: GLInput<'_>,
        render_frame: &mut impl FnMut(&mut DefaultSurface, Vec<Texture2d>) -> Result<(), Box<dyn Error>>,
    ) {
        unsafe {
            self.ctx.rebuild(self.backend.clone()).unwrap();
            // make glium think it's drawing to the default framebuffer
        };

        // let rb = RenderBuffer::new(
        //     &self.ctx,
        //     glium::texture::UncompressedFloatFormat::U8U8U8U8,
        //     render_res.0,
        //     render_res.1,
        // )
        // .expect("RenderBuffer could not be created");

        // // let tx = Texture2d::empty_with_format(
        // //     &self.ctx,
        // //     glium::texture::UncompressedFloatFormat::U8U8U8U8,
        // //     glium::texture::MipmapsOption::NoMipmap,
        // //     render_res.0,
        // //     render_res.1,
        // // )
        // // .expect("Texture2d could not be created");

        // let mut fb =
        //     SimpleFrameBuffer::new(&self.ctx, &rb).expect("SimpleFrameBuffer could not be created");

        // fb.clear_color(0.0, 0.0, 0.0, 0.0);
        // let x = SimpleFrameBuffer::

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

        let mut frame = Frame::new(self.ctx.clone(), out_res);

        if let Err(err) = render_frame(&mut frame, textures) {
            tracing::error!("Render ERROR: {err:?}");
        }

        // frame.clear_color(0.0, 0.0, 0.0, 0.0);
        // self.set_default_db_to_ffgl_fb(&frame_data);

        // let empty = EmptyFrameBuffer::new(&self.ctx, render_res.0, render_res.1, None, None, false);

        //puts the texture into the framebuffer
        // let id = fb.get_id();
        // let id = rb.get_id();
        // fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // trace!("")
        trace!(?out_res, ?render_res, "RENDERED");
        // frame.clear_color(0.0, 0.0, 0.0, 0.0);

        //tell glium to draw to the default framebuffer
        // fb.fill(&frame, glium::uniforms::MagnifySamplerFilter::Nearest);

        // let blit_target_size = output_res;

        // debug!("BLITTING {render_res:?} -> {blit_target_size:?}");

        unsafe {
            gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);

            blit_fb(render_res, out_res);

            self.ctx.rebuild(self.backend.clone()).unwrap();
            // make glium think it's drawing to the default framebuffer
        };

        frame.finish().unwrap();

        // unsafe {
        //     // gl::BindFramebuffer(gl::READ_FRAMEBUFFER, 0);
        //     gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);
        //     blit_fb(render_res, blit_target_size);
        // }

        //reset to what host expects
        // gl_reset(frame_data);
        // validate::validate_context_state();

        // validate_viewport(&viewport);
    }

    // use this before a draw call to make glium think it's drawing to the default framebuffer
    fn set_default_db_to_ffgl_fb(&self, frame_data: &GLInput<'_>) {
        self.ctx.swap_buffers().expect("swap_buffers failed");
        // actually draw to frame_data.host
        unsafe {
            // gl::Clear(gl::COLOR_BUFFER_BIT);
            gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, frame_data.host);
        }
    }
}

// Swaps the buffers between the default and the given id
// unsafe fn swap_buffers(id: i32) {
//     gl::BindFramebuffer(gl::READ_FRAMEBUFFER, id);
//     gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, 0);
//     gl::BlitFramebuffer(
//         0,
//         0,
//         1920,
//         1080,
//         0,
//         0,
//         1920,
//         1080,
//         gl::COLOR_BUFFER_BIT,
//         gl::NEAREST,
//     );
// }

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

    // https://registry.khronos.org/OpenGL-Refpages/es3.0/html/glBlitFramebuffer.xhtml#:~:text=glBlitFramebuffer%20transfers%20a%20rectangle%20of,GL_COLOR_BUFFER_BIT%20%2C%20GL_DEPTH_BUFFER_BIT%20%2C%20and%20GL_STENCIL_BUFFER_BIT%20.
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
