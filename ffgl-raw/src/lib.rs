pub mod conversions;
mod ffgl_derive;
pub mod ffi;
mod instance;
pub mod log;
pub mod parameters;
pub use instance::FFGLData;
pub mod validate;

use ffi::ffgl2::*;

pub use parameters::ParamInfo;

use core::slice;
use std::{
    cell::OnceCell,
    ffi::{c_void, CStr, CString},
    fmt::Debug,
    sync::OnceLock,
};

pub use conversions::*;
pub use log::{FFGLLogger, LOADING_LOGGER};

pub use num_traits::ToPrimitive;

pub struct Instance<T> {
    data: FFGLData,
    renderer: T,
}

impl<T> Debug for Instance<T> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Instance")
            .field("data", &self.data)
            .field("renderer", &std::any::type_name::<T>())
            .finish()
    }
}

pub trait ParamHandler {
    type Param: ParamInfo + 'static;

    fn num_params() -> usize {
        0
    }
    fn param_info(index: usize) -> &'static Self::Param;

    fn get_param(&self, index: usize) -> f32;
    fn set_param(&mut self, index: usize, value: f32);
}

pub trait FFGLHandler: Debug + ParamHandler {
    ///Called by [Op::FF_GETINFO] to get the plugin info
    ///Should only ever be called once
    unsafe fn init() -> PluginInfo;

    ///Called by [Op::FF_INSTANTIATEGL] to create a new instance of the plugin
    unsafe fn new(inst_data: &FFGLData) -> Self;

    ///Called by [Op::FF_PROCESSOPENGL] to draw the plugin
    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput);
}

pub trait NoParamsHandler {}

impl<T: NoParamsHandler> ParamHandler for T {
    type Param = parameters::BasicParamInfo;

    fn param_info(_index: usize) -> &'static Self::Param {
        unimplemented!("No params")
    }

    fn get_param(&self, _index: usize) -> f32 {
        unimplemented!("No params")
    }

    fn set_param(&mut self, _index: usize, _value: f32) {
        unimplemented!("No params")
    }
}

fn param<T: FFGLHandler>(_instance: Option<&mut Instance<T>>, index: FFGLVal) -> &'static T::Param {
    &T::param_info(unsafe { index.num as usize })
}

static mut INITIALIZED: bool = false;
static mut INFO: Option<PluginInfoStruct> = None;
static mut ABOUT: Option<CString> = None;
static mut DESCRIPTION: Option<CString> = None;
static mut INFO_EXTENDED: Option<PluginExtendedInfoStruct> = None;

use tracing::{debug, error, info, span, trace, warn, Level};

pub fn default_ffgl_callback<T: FFGLHandler + 'static>(
    function: Op,
    mut input_value: FFGLVal,
    instance: Option<&mut Instance<T>>,
) -> FFGLVal {
    let noisy_op = match function {
        Op::ProcessOpenGL
        | Op::SetBeatInfo
        | Op::SetTime
        | Op::GetParameterEvents
        | Op::SetParameter
        | Op::GetParameterDisplay
        | Op::GetParameterType => true,
        _ => false,
    };

    if !noisy_op {
        info!("Op::{function:?}({})", unsafe { input_value.num });
    } else {
        trace!("Op::{function:?}({})", unsafe { input_value.num });
    }

    unsafe {
        if !INITIALIZED {
            INITIALIZED = true;
            info!("INITIALIZING");

            let info = T::init();

            ABOUT = Some(CString::new(info.about).unwrap());
            DESCRIPTION = Some(CString::new(info.description).unwrap());

            INFO = Some(plugin_info(
                std::mem::transmute(&info.unique_id),
                std::mem::transmute(&info.name),
                info.ty,
            ));

            INFO_EXTENDED = Some(plugin_info_extended(
                ABOUT.as_ref().unwrap(),
                DESCRIPTION.as_ref().unwrap(),
            ));
        }
    }

    let resp = match function {
        Op::GetPluginCaps => {
            let cap_num = unsafe { input_value.num };
            let cap = num::FromPrimitive::from_u32(cap_num).expect("Unexpected cap n{cap_num}");

            let result = match cap {
                PluginCapacity::MinInputFrames => FFGLVal { num: 0 },
                PluginCapacity::MaxInputFrames => FFGLVal { num: 1 },

                PluginCapacity::ProcessOpenGl => SupportVal::Supported.into(),
                PluginCapacity::SetTime => SupportVal::Supported.into(),

                PluginCapacity::TopLeftTextureOrientation => SupportVal::Supported.into(),

                _ => SupportVal::Unsupported.into(),
            };

            debug!("{cap:?} => {}", unsafe { result.num });

            result
        }

        Op::EnablePluginCap => {
            let cap_num = unsafe { input_value.num };
            let cap = num::FromPrimitive::from_u32(cap_num).expect("Unexpected cap n{cap_num}");

            let result: FFGLVal = match cap {
                PluginCapacity::TopLeftTextureOrientation => SuccessVal::Success.into(),
                _ => SuccessVal::Fail.into(),
            };

            debug!("{cap:?} => {}", unsafe { result.num });

            result
        }

        // Op::GetNumParameters => FFGLVal { num: 0 },
        Op::GetNumParameters => FFGLVal {
            num: T::num_params() as u32,
        },

        Op::GetParameterDefault => param(instance, input_value).default_val().into(),
        Op::GetParameterGroup => {
            let input: &GetStringStructTag = unsafe { input_value.as_ref() };
            let buffer = input.stringBuffer;

            let group = T::param_info(input.parameterNumber as usize).group();

            let string_target: &mut [char] = unsafe {
                slice::from_raw_parts_mut(buffer.address as *mut char, buffer.maxToWrite as usize)
            };

            let copied_chars = std::cmp::min(group.len(), buffer.maxToWrite as usize);

            string_target[..copied_chars]
                .copy_from_slice(&group[..copied_chars].chars().collect::<Vec<_>>());

            debug!("GET PARAM GROUP {group:?}");

            SuccessVal::Success.into()
        }
        Op::GetParameterDisplay => param(instance, input_value).display_name().into(),
        Op::GetParameterName => param(instance, input_value).name().into(),
        Op::GetParameterType => param(instance, input_value).param_type().into(),

        Op::GetParameter => instance
            .unwrap()
            .renderer
            .get_param(unsafe { input_value.num } as usize)
            .into(),

        Op::SetParameter => {
            let input: &SetParameterStruct = unsafe { input_value.as_ref() };
            let index = input.ParameterNumber;

            // let param = param_mut(instance, index as usize);
            let index_usize = index as usize;

            //dunno why they store this in a u32, whatever..
            let new_value =
                unsafe { std::mem::transmute::<u32, f32>(input.NewParameterValue.UIntValue) };

            // log::logln!("SET PARAM cb {index_usize:?}=>{new_value:#?}");

            instance.unwrap().renderer.set_param(index_usize, new_value);

            // set_param(instance, index as usize, ParamValue::Float(new_value));

            // log::logln!(
            //     "SET PARAM {param:?} {old_value:?} => {new_value:?}",
            //     param = param.display_name.to_str().unwrap(),
            // );

            SuccessVal::Success.into()
        }
        Op::GetParameterRange => {
            let input: &mut GetRangeStruct = unsafe { (input_value).as_mut() };

            let index = input.parameterNumber;
            let param = &T::param_info(index as usize);

            input.range = RangeStruct {
                min: param.min(),
                max: param.max(),
            };

            SuccessVal::Success.into()
        }
        // Op::GetParameterGroup => param(instance, ffgl2::GetParameterGroupStruct).group.into(),
        Op::GetInfo => unsafe { INFO.as_ref().unwrap().into() },

        Op::GetExtendedInfo => unsafe { INFO_EXTENDED.as_ref().unwrap().into() },

        Op::InstantiateGL => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };

            let data = FFGLData::new(viewport);
            let renderer = unsafe { T::new(&data) };
            let instance = Instance { data, renderer };

            debug!("INSTGL\n{instance:#?}");

            FFGLVal::from_static(Box::leak(Box::<Instance<T>>::new(instance)))
        }

        // Op::FF_RESIZE => {
        //     let inst = instance.unwrap();
        //     inst.data.viewport = unsafe { inputValue.as_ref() };
        // }
        Op::DeinstantiateGL => {
            let inst = instance.unwrap();

            debug!("DEINSTGL\n{inst:#?}");
            unsafe {
                drop(Box::from_raw(inst as *mut Instance<T>));
            }

            SuccessVal::Success.into()
        }

        Op::ProcessOpenGL => {
            let gl_process_info: &ProcessOpenGLStruct = unsafe { input_value.as_ref() };

            // logln!("PROCESSGL info \n{gl_process_info:#?}");

            let Instance { data, renderer } = instance.unwrap();
            let gl_input = gl_process_info.into();

            // logln!("PROCESSGL input \n{gl_input:?}");

            unsafe { renderer.draw(&data, gl_input) };

            SuccessVal::Success.into()
        }

        Op::SetTime => {
            let seconds: f64 = *unsafe { input_value.as_ref() };
            instance.unwrap().data.set_time(seconds);
            SuccessVal::Success.into()
        }

        //This is called before GLInitialize
        Op::SetBeatInfo => {
            let beat_info: &SetBeatinfoStruct = unsafe { input_value.as_ref() };
            if let Some(instance) = instance {
                instance.data.set_beat(*beat_info);
            }
            SuccessVal::Success.into()
        }

        Op::Resize => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };
            debug!("RESIZE\n{viewport:#?}");
            // instance.unwrap().data.set_viewport(viewport);
            SuccessVal::Success.into()
        }

        Op::Connect => SuccessVal::Success.into(),

        Op::Instantiate | Op::Deinstantiate | Op::ProcessFrame | Op::ProcessFrameCopy => {
            SuccessVal::Fail.into()
        }

        Op::InitialiseV2 => SuccessVal::Success.into(),
        Op::Initialise => SuccessVal::Success.into(),
        Op::Deinitialise => SuccessVal::Success.into(),

        _ => SuccessVal::Fail.into(),
    };

    if !noisy_op {
        info!("=> {}", unsafe { resp.num });
    } else {
        trace!("=> {}", unsafe { resp.num });
    }

    resp
}
