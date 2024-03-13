//! This module provides the main traits for creating FFGL plugins
//! Use [simplified] for a simpler way to create plugins

use parameters::ParamInfo;

use std;

use std::error::Error;
use std::fmt::Debug;

use crate::inputs::FFGLData;

use crate::{info, inputs::GLInput, parameters};

#[doc(hidden)]
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
pub trait FFGLInstance {
    fn get_param(&self, index: usize) -> f32;
    fn set_param(&mut self, index: usize, value: f32);

    ///Called by [crate::conversions::Op::ProcessOpenGL] to draw the plugin
    fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput);
}

///This type is created once per plugin load.
/// You can use it to store static state and create instances
pub trait FFGLHandler {
    type Instance: FFGLInstance;
    type NewInstanceError: Error + Send + Sync + 'static;
    // type Param: ParamInfo + 'static;

    ///Only called once per plugin
    fn init() -> Self;

    fn num_params(&'static self) -> usize;

    fn param_info(&'static self, index: usize) -> &'static dyn ParamInfo;

    fn plugin_info(&'static self) -> info::PluginInfo;

    fn new_instance(
        &'static self,
        inst_data: &FFGLData,
    ) -> Result<Self::Instance, Self::NewInstanceError>;
}

pub mod simplified;
