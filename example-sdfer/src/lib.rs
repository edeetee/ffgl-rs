use ffgl_core::{handler::simplified::*, plugin_main};

struct Instance;

impl SimpleFFGLInstance for Instance {
    fn new(inst_data: &ffgl_core::FFGLData) -> Self {
        todo!()
    }

    fn plugin_info() -> ffgl_core::info::PluginInfo {
        todo!()
    }

    fn draw(&mut self, inst_data: &ffgl_core::FFGLData, frame_data: ffgl_core::GLInput) {
        todo!()
    }

    fn num_params() -> usize {
        1
    }

    fn param_info(_index: usize) -> &'static ffgl_core::parameters::BasicParamInfo {
        panic!("No params")
    }

    fn get_param(&self, _index: usize) -> f32 {
        panic!("No params")
    }

    fn set_param(&mut self, _index: usize, _value: f32) {
        panic!("No params")
    }
}

plugin_main!(SimpleFFGLHandler<Instance>);
