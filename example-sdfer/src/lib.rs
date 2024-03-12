use ffgl_core::{
    handler::simplified::*,
    info::{plugin_info, PluginInfo},
    plugin_main,
};
use ffgl_glium::FFGLGlium;
use glium::Texture2d;

//https://github.com/LykenSol/sdfer/tree/main/src
struct SdferInstance {
    // input_file: std::path::PathBuf,
    sdf: sdfer::Image2d<sdfer::Unorm8>,
    buffers: sdfer::esdt::ReusableBuffers,
    glium: ffgl_glium::FFGLGlium,
}

// impl SdferInstance {
//     fn process(&mut self, texture: Texture2d) {
//         let img = sdfer::Image2d(path).unwrap().to_rgba8();
//         let sdf = sdfer::Image2d::from_image(&img);
//         self.sdf = sdf;
//     }
// }

impl SimpleFFGLInstance for SdferInstance {
    fn new(inst_data: &ffgl_core::FFGLData) -> Self {
        Self {
            sdf: Default::default(),
            buffers: Default::default(),
            glium: FFGLGlium::new(inst_data),
        }
    }

    fn plugin_info() -> ffgl_core::info::PluginInfo {
        PluginInfo {
            unique_id: *b"SDFR",
            name: *b"Sdfer           ",
            ty: ffgl_core::info::PluginType::Source,
            ..Default::default()
        }
    }

    fn draw(&mut self, inst_data: &ffgl_core::FFGLData, frame_data: ffgl_core::GLInput) {
        // let res = inst_data.get_resolution();

        // self.glium
        //     .draw(output_res, render_res, frame_data, render_frame)
    }

    fn num_params() -> usize {
        1
    }

    fn param_info(_index: usize) -> &'static ffgl_core::parameters::SimpleParamInfo {
        panic!("No params")
    }

    fn get_param(&self, _index: usize) -> f32 {
        panic!("No params")
    }

    fn set_param(&mut self, _index: usize, _value: f32) {
        panic!("No params")
    }
}

plugin_main!(SimpleFFGLHandler<SdferInstance>);
