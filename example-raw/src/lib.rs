mod test_empty;
mod test_gl;
use ffgl_raw;
mod shader_helper;

ffgl_raw::ffgl_handler!(test_gl::TestGl);
