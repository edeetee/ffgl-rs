mod test_empty;
mod test_gl;
use ffgl_raw::{self, traits::SimpleFFGLHandler};
mod shader_helper;

ffgl_raw::ffgl_handler!(SimpleFFGLHandler<test_gl::TestGl>);
