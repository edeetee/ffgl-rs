use ffgl_glium::{ffgl_handler, handler::simplified::*};

struct Instance;

impl SimpleFFGLInstance for Instance {
    fn new(inst_data: &ffgl_glium::FFGLData) -> Self {
        todo!()
    }

    fn plugin_info() -> ffgl_glium::info::PluginInfo {
        todo!()
    }

    fn draw(&mut self, inst_data: &ffgl_glium::FFGLData, frame_data: ffgl_glium::GLInput) {
        todo!()
    }

    fn num_params() -> usize {
        1
    }

    fn param_info(_index: usize) -> &'static ffgl_glium::parameters::BasicParamInfo {
        panic!("No params")
    }

    fn get_param(&self, _index: usize) -> f32 {
        panic!("No params")
    }

    fn set_param(&mut self, _index: usize, _value: f32) {
        panic!("No params")
    }
}

ffgl_handler!(SimpleFFGLHandler<Instance>);
