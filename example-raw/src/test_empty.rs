use core::panic;

use ffgl_core::handler::simplified::SimpleFFGLInstance;
use ffgl_core::info::{PluginInfo, PluginType};
use ffgl_core::FFGLData;

#[derive(Debug)]
pub struct EmptyFFGLHandler;

impl SimpleFFGLInstance for EmptyFFGLHandler {
    fn new(_inst_data: &FFGLData) -> Self {
        Self
    }

    fn plugin_info() -> PluginInfo {
        PluginInfo {
            unique_id: [0, 0, 0, 0],
            name: *b"EmptyFFGLPlugin ",
            ty: PluginType::Source,
            about: "".to_string(),
            description: "".to_string(),
        }
    }
    fn get_param(&self, _index: usize) -> f32 {
        panic!("No params")
    }

    fn set_param(&mut self, _index: usize, _value: f32) {
        panic!("No params")
    }

    fn draw(&mut self, _inst_data: &FFGLData, _frame_data: ffgl_core::GLInput) {}
}
