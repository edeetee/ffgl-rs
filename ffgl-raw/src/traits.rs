use parameters::BasicParamInfo;

use parameters::ParamInfo;

use std;

use std::fmt::Debug;

use instance::FFGLData;

use crate::{instance, parameters, GLInput, PluginInfo};

pub struct Instance<T> {
    pub(crate) data: FFGLData,
    pub(crate) renderer: T,
}

impl<I> Debug for Instance<I> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Instance")
            .field("data", &self.data)
            .field("renderer", &std::any::type_name::<I>())
            .finish()
    }
}

///This type is created once per instance of a plugin
pub trait FFGLInstance: Debug {
    fn get_param(&self, index: usize) -> f32;
    fn set_param(&mut self, index: usize, value: f32);

    ///Called by [Op::FF_PROCESSOPENGL] to draw the plugin
    fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput);
}

///This type is created once per plugin
/// You can use it to store static state and create instances
pub trait FFGLHandler {
    type Instance: FFGLInstance;
    type Param: ParamInfo + 'static;

    ///Only called once per plugin
    fn init() -> Self;

    fn num_params(&'static self) -> usize;

    fn param_info(&'static self, index: usize) -> &'static Self::Param;

    fn plugin_info(&'static self) -> PluginInfo;

    fn new_instance(&'static self, inst_data: &FFGLData) -> Self::Instance;
}

///This is a handler that just delegates to a SimpleFFGLInstance
pub struct SimpleFFGLHandler<T: SimpleFFGLInstance> {
    _marker: std::marker::PhantomData<T>,
}

///Implement this trait for a plugin without any static state
pub trait SimpleFFGLInstance: FFGLInstance {
    fn new(inst_data: &FFGLData) -> Self;

    fn num_params() -> usize {
        0
    }
    fn param_info(index: usize) -> &'static BasicParamInfo {
        panic!("No params")
    }

    fn plugin_info() -> PluginInfo;

    fn get_param(&self, index: usize) -> f32 {
        panic!("No params")
    }
    fn set_param(&mut self, index: usize, value: f32) {
        panic!("No params")
    }

    ///Called by [Op::FF_PROCESSOPENGL] to draw the plugin
    fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput);
}

impl<T: SimpleFFGLInstance> FFGLInstance for T {
    fn get_param(&self, index: usize) -> f32 {
        SimpleFFGLInstance::get_param(self, index)
    }

    fn set_param(&mut self, index: usize, value: f32) {
        SimpleFFGLInstance::set_param(self, index, value)
    }

    fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput) {
        SimpleFFGLInstance::draw(self, inst_data, frame_data)
    }
}

impl<T: SimpleFFGLInstance> FFGLHandler for SimpleFFGLHandler<T> {
    type Instance = T;
    type Param = BasicParamInfo;

    fn init() -> Self {
        Self {
            _marker: std::marker::PhantomData,
        }
    }

    fn num_params(&self) -> usize {
        T::num_params()
    }

    fn param_info(&self, index: usize) -> &'static Self::Param {
        T::param_info(index)
    }

    fn plugin_info(&self) -> PluginInfo {
        T::plugin_info()
    }

    fn new_instance(&self, inst_data: &FFGLData) -> Self::Instance {
        T::new(inst_data)
    }
}
