use ffgl::{parameters::BasicParam};

use crate::shader_helper::*;
use std::{
    ffi::{CString},
    ptr::{self},
};

use crate::{ffgl::FFGLData, ffgl::FFGLHandler};
use gl::{self, types::*};

#[derive(Debug)]
pub struct TestGl {
    vertex_array_id: GLuint,
    vertex_buffer_id: GLuint,
    program_id: GLuint,
    beat_uniform_id: GLint,
}

// Shader sources
pub static VS_SRC: &'static str = "
#version 150
in vec2 position;
out vec2 v_pos2;
uniform float beat;
out float v_beat;

void main() {
    v_pos2 = position;
    v_beat = beat;
    gl_Position = vec4(position * sin(beat*6.28318), 0.0, 1.0);
}";

pub static FS_SRC: &'static str = "
#version 150
in vec2 v_pos2;
in float v_beat;
out vec4 out_color;

void main() {
    out_color = vec4(gl_FragCoord.xy/1000.0 - v_pos2, 0.0, 1.0);
}";

impl FFGLHandler for TestGl {
    unsafe fn new(_data: &FFGLData) -> Self {
        gl_loader::init_gl();
        gl::load_with(|s| gl_loader::get_proc_address(s).cast());

        let vs = compile_shader(VS_SRC, gl::VERTEX_SHADER);
        let fs = compile_shader(FS_SRC, gl::FRAGMENT_SHADER);
        let program_id = link_program(vs, fs);

        let mut vertex_array_id = 0;
        let mut vertex_buffer_id = 0;

        gl::GenVertexArrays(1, &mut vertex_array_id);
        gl::BindVertexArray(vertex_array_id);

        gl::GenBuffers(1, &mut vertex_buffer_id);
        gl::BindBuffer(gl::ARRAY_BUFFER, vertex_buffer_id);

        static VERTEX_DATA: [GLfloat; 6] = [0.0, 0.5, 0.5, -0.5, -0.5, -0.5];

        // Give our vertices to OpenGL.
        gl::BufferData(
            gl::ARRAY_BUFFER,
            (VERTEX_DATA.len() * std::mem::size_of::<f32>()) as isize,
            (&VERTEX_DATA).as_ptr().cast(),
            gl::STATIC_DRAW,
        );

        // Use shader program
        gl::UseProgram(program_id);
        let out_frag_name = CString::new("out_color").unwrap();
        gl::BindFragDataLocation(program_id, 0, out_frag_name.as_ptr());

        let uniform_name = CString::new("beat").unwrap();
        let beat_uniform_id = gl::GetUniformLocation(program_id, uniform_name.as_ptr());

        // Specify the layout of the vertex data
        let vert_pos_name = CString::new("position").unwrap();
        let pos_attr = gl::GetAttribLocation(program_id, vert_pos_name.as_ptr());
        gl::EnableVertexAttribArray(pos_attr as GLuint);
        gl::VertexAttribPointer(
            pos_attr as GLuint,
            2,
            gl::FLOAT,
            gl::FALSE as GLboolean,
            0,
            ptr::null(),
        );

        Self {
            vertex_array_id,
            vertex_buffer_id,
            program_id,
            beat_uniform_id,
        }
    }

    unsafe fn draw(&mut self, data: &FFGLData, _frame_data: &ffgl::ffgl::ProcessOpenGLStruct) {
        //most basic test here
        gl::ClearColor(
            data.host_beat.barPhase,
            data.host_beat.barPhase * 3.123,
            0.0,
            1.0,
        );
        gl::Clear(gl::COLOR_BUFFER_BIT);

        gl::UseProgram(self.program_id);

        gl::Uniform1f(self.beat_uniform_id, data.host_beat.barPhase);

        gl::BindVertexArray(self.vertex_array_id);

        gl::DrawArrays(gl::TRIANGLES, 0, 3); // Starting from vertex 0; 3 vertices total -> 1 triangle

        gl::BindVertexArray(0);

        // validate::validate_context_state();
        // gl::UseProgram(0);
    }

    type Param = BasicParam;
}

impl Drop for TestGl {
    fn drop(&mut self) {
        unsafe {
            gl::DeleteBuffers(1, &self.vertex_buffer_id);
            gl::DeleteVertexArrays(1, &self.vertex_array_id);
        }
    }
}
