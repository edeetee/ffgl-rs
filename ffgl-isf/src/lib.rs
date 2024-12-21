mod fullscreen_shader;
mod param;
mod shader;
mod util;

pub mod handler;
pub mod instance;

ffgl_core::plugin_main!(handler::IsfFFGLState);
