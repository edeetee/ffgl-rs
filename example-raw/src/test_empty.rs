

use ffgl::parameters::BasicParam;

use crate::{ffgl::FFGLData, ffgl::FFGLHandler};

#[derive(Debug)]
pub struct TestEmpty;

impl FFGLHandler for TestEmpty {
    unsafe fn new(_data: &FFGLData) -> Self {
        Self
    }

    unsafe fn draw(&mut self, _data: &FFGLData, _frame_data: &ffgl::ProcessOpenGLStruct) {}

    type Param = BasicParam;
}
