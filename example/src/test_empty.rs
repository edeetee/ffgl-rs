use std::{
    ffi::c_void,
    mem::{size_of, size_of_val},
    os::raw,
    ptr::null,
};

use crate::{ffgl::FFGLData, ffgl::FFGLHandler};

#[derive(Debug)]
pub struct TestEmpty;

impl FFGLHandler for TestEmpty {
    unsafe fn new(data: &FFGLData) -> Self {
        Self
    }

    unsafe fn draw(&mut self, data: &FFGLData, frame_data: &ffgl::ProcessOpenGLStruct) {}
}
