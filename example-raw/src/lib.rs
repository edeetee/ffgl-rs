mod test_empty;
mod test_gl;
use ffgl_core::{self, handler::simplified::SimpleFFGLHandler};
mod shader_helper;
mod validate;

ffgl_core::plugin_main!(SimpleFFGLHandler<test_gl::TestGl>);
