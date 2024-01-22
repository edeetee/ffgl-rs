use ffgl_raw::parameters::BasicParamInfo;

use ffgl_raw::{plugin_info, FFGLData, FFGLHandler, NoParamsHandler, PluginInfo};

#[derive(Debug)]
pub struct TestEmpty;

impl NoParamsHandler for TestEmpty {}

impl FFGLHandler for TestEmpty {
    unsafe fn new(_data: &FFGLData) -> Self {
        Self
    }

    unsafe fn draw(&mut self, _data: &FFGLData, _frame_data: &ffgl::ProcessOpenGLStruct) {}

    unsafe fn info() -> PluginInfo {
        PluginInfo {
            unique_id: 0000,
            name: "Example Empty from ffgl-rs",
            ty: ffgl_raw::PluginType::Source,
            about: "",
            description: "",
        }
    }
}
