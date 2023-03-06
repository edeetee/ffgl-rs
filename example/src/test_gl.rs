use std::{mem::{size_of_val, size_of}, ffi::c_void, os::raw, ptr::null};

use crate::{ffgl::ffi::gl, ffgl::FFGLData, ffgl::FFGLHandler};

#[derive(Debug)]
pub struct TestGl {
    vertex_array_id: u32,
    vertex_buffer_id: u32
}

impl FFGLHandler for TestGl {
    unsafe fn new(data: &FFGLData) -> Self {
        let mut vertex_array_id = 0;
        let mut vertex_buffer_id = 0;

        gl::GenVertexArrays(1, &mut vertex_array_id);
        gl::BindVertexArray(vertex_array_id);
        
        gl::GenBuffers(1, &mut vertex_buffer_id);
        gl::BindBuffer(gl::ARRAY_BUFFER, vertex_buffer_id);

        let vertex_buffer_data: [f32;9] = [
            -1.0, -1.0, 0.0,
            1.0, -1.0, 0.0,
            0.0,  1.0, 0.0,
        ];

        // Give our vertices to OpenGL.
        gl::BufferData(gl::ARRAY_BUFFER, size_of_val(&vertex_buffer_data) as isize, (&vertex_buffer_data).as_ptr().cast(), gl::STATIC_DRAW);
        
        gl::VertexAttribPointer(
            0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
            3,                  // size
            gl::FLOAT,           // type
            gl::FALSE,           // normalized?
            0,                  // stride
            null()            // array buffer offset
        );
        gl::EnableVertexAttribArray(0);

        gl::BindVertexArray(0);
        gl::BindBuffer(gl::ARRAY_BUFFER, 0);

        Self {
            vertex_array_id,
            vertex_buffer_id
        }
    }

    unsafe fn draw(&mut self, data: &FFGLData, frame_data: &ffgl::ProcessOpenGLStruct) {
        //most basic test here
        gl::ClearColor(data.host_beat.barPhase, data.host_beat.barPhase*3.123, 0.0, 1.0);
        gl::Clear(gl::COLOR_BUFFER_BIT);

        gl::BindVertexArray(self.vertex_array_id);
        // gl::BindBuffer(gl::ARRAY_BUFFER, self.vertex_buffer_id);

        gl::UseProgram(0);
        gl::DrawArrays(gl::TRIANGLES, 0, 3); // Starting from vertex 0; 3 vertices total -> 1 triangle

        gl::BindVertexArray(0);
        // gl::BindBuffer(gl::ARRAY_BUFFER, 0);
    }
}

impl Drop for TestGl {
    fn drop(&mut self) {
        unsafe {
            gl::DeleteBuffers(1, &self.vertex_buffer_id);
            gl::DeleteVertexArrays(1, &self.vertex_array_id);
        }
    }
}