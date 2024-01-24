use core::panic;

use ffgl_core::parameters::BasicParamInfo;

use ffgl_core::traits::{FFGLHandler, FFGLInstance, SimpleFFGLInstance};
use ffgl_core::{FFGLData, PluginInfo};

#[derive(Debug)]
pub struct EmptyFFGLHandler;

impl SimpleFFGLInstance for EmptyFFGLHandler {
    fn new(inst_data: &FFGLData) -> Self {
        Self
    }

    fn plugin_info() -> PluginInfo {
        PluginInfo {
            unique_id: [0, 0, 0, 0],
            name: *b"EmptyFFGLPlugin ",
            ty: ffgl_core::PluginType::Source,
            about: "".to_string(),
            description: "".to_string(),
        }
    }
    fn get_param(&self, index: usize) -> f32 {
        panic!("No params")
    }

    fn set_param(&mut self, index: usize, value: f32) {
        panic!("No params")
    }

    fn draw(&mut self, inst_data: &FFGLData, frame_data: ffgl_core::GLInput) {}
}
