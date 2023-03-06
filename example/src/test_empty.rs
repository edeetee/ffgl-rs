use std::{mem::{size_of_val, size_of}, ffi::c_void, os::raw, ptr::null};

use crate::{ffgl::ffi::gl, ffgl::FFGLData, ffgl::FFGLHandler};

#[derive(Debug)]
pub struct TestEmpty;

impl FFGLHandler for TestEmpty {
    unsafe fn new(data: &FFGLData) -> Self {
        Self
    }

    unsafe fn draw(&mut self, data: &FFGLData, frame_data: &ffgl::ProcessOpenGLStruct) {

    }
}