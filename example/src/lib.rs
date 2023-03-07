mod gl;
mod test_empty;
mod test_gl;
use ffgl;
mod shader_helper;

ffgl::ffgl_handler!(test_gl::TestGl);
