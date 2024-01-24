mod test_empty;
mod test_gl;
use ffgl_core::{self, traits::SimpleFFGLHandler};
mod shader_helper;

ffgl_core::ffgl_handler!(SimpleFFGLHandler<test_gl::TestGl>);
