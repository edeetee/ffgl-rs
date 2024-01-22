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
use std::{ffi::c_void, fmt::Debug};

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

#[derive(PartialEq)]
enum FFGLVersion {
    // V1_5,
    V2_1,
}

impl FFGLVersion {
    const fn major(&self) -> u32 {
        match self {
            // FFGLVersion::V1_5 => 1,
            FFGLVersion::V2_1 => 2,
        }
    }

    const fn minor(&self) -> u32 {
        match self {
            // FFGLVersion::V1_5 => 5,
            FFGLVersion::V2_1 => 1,
        }
    }
}

const FFGL_VERSION: FFGLVersion = FFGLVersion::V2_1;

// static mut INFO: ffgl::PluginInfoStruct = plugin_info(b"TRP0", b"testrustplugin  ");
// static mut INFO_EXTENDED: ffgl::PluginExtendedInfoStruct =

pub fn plugin_info(
    unique_id: &[u8; 4],
    name: &[u8; 16],
    plugin_type: PluginType,
) -> PluginInfoStruct {
    PluginInfoStruct {
        APIMajorVersion: FFGL_VERSION.major(),
        APIMinorVersion: FFGL_VERSION.minor(),
        PluginUniqueID: *unsafe { std::mem::transmute::<_, &[i8; 4]>(unique_id) },
        PluginName: *unsafe { std::mem::transmute::<_, &[i8; 16]>(name) },
        PluginType: plugin_type.to_u32().unwrap(),
    }
}

pub const fn plugin_info_extended(
    about: &'static str,
    description: &'static str,
) -> PluginExtendedInfoStruct {
    PluginExtendedInfoStruct {
        PluginMajorVersion: 0,
        PluginMinorVersion: 0,
        Description: about.as_ptr().cast_mut().cast(),
        About: description.as_ptr().cast_mut().cast(),
        FreeFrameExtendedDataSize: 0,
        FreeFrameExtendedDataBlock: std::ptr::null::<c_void>() as *mut c_void,
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

use once_cell::unsync::Lazy;

pub trait FFGLHandler: Debug + ParamHandler {
    unsafe fn info() -> &'static PluginInfoStruct {
        static mut INFO: Lazy<PluginInfoStruct> =
            Lazy::new(|| plugin_info(b"TRP0", b"testrustplugin  ", PluginType::Source));
        &mut INFO
    }

    unsafe fn info_extended() -> &'static mut PluginExtendedInfoStruct {
        static mut INFO_EXTENDED: PluginExtendedInfoStruct =
            plugin_info_extended("Edward Taylor\0", "Built with Rust\0");
        &mut INFO_EXTENDED
    }

    ///Called by [Op::FF_INSTANTIATEGL] to create a new instance of the plugin
    unsafe fn new(inst_data: &FFGLData) -> Self;

    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: GLInput);
}

// fn params<T: FFGLHandler>(instance: Option<&mut Instance<T>>) -> &'static [T::Param] {
//     T::params()
// }

// fn params<T: FFGLHandler<P>, P: Param>(instance: Option<&mut Instance<T>>) -> &[BasicParam] {
//     &TEST_PARAMS
// }

fn param<T: FFGLHandler>(_instance: Option<&mut Instance<T>>, index: FFGLVal) -> &'static T::Param {
    &T::param_info(unsafe { index.num as usize })
}

// fn set_param<T: FFGLHandler>(instance: Option<&mut Instance<T>>, index: usize, value: ParamValue) {
//     println!("SET PARAM fn {index} {value:?}");
//     instance.unwrap().renderer.set_param(index, value)
// }

// const TEST_PARAMS: &'static [BasicParam] = &[];

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
        // | Op::SetParameter
        // | Op::GetParameterDisplay
        | Op::GetParameterType => true,
        _ => false,
    };

    if !noisy_op {
        log::logln!("Op::{function:?}({})", unsafe { input_value.num });
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

            log::logln!("{cap:?} => {}", unsafe { result.num });

            result
        }

        Op::EnablePluginCap => {
            let cap_num = unsafe { input_value.num };
            let cap = num::FromPrimitive::from_u32(cap_num).expect("Unexpected cap n{cap_num}");

            let result: FFGLVal = match cap {
                PluginCapacity::TopLeftTextureOrientation => SuccessVal::Success.into(),
                _ => SuccessVal::Fail.into(),
            };

            log::logln!("{cap:?} => {}", unsafe { result.num });

            result
        }

        // Op::GetNumParameters => FFGLVal { num: 0 },
        Op::GetNumParameters => FFGLVal {
            num: T::num_params() as u32,
        },

        Op::GetParameterDefault => param(instance, input_value).default().into(),
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

            log::logln!("GET PARAM GROUP {group:?}");

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
        Op::GetInfo => unsafe { T::info().into() },

        Op::GetExtendedInfo => unsafe { T::info_extended().into() },

        Op::InstantiateGL => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };

            let data = FFGLData::new(viewport);
            let renderer = unsafe { T::new(&data) };
            let instance = Instance { data, renderer };

            log::logln!("INSTGL\n{instance:#?}");

            FFGLVal::from_static(Box::leak(Box::<Instance<T>>::new(instance)))
        }

        // Op::FF_RESIZE => {
        //     let inst = instance.unwrap();
        //     inst.data.viewport = unsafe { inputValue.as_ref() };
        // }
        Op::DeinstantiateGL => {
            let inst = instance.unwrap();

            log::logln!("DEINSTGL\n{inst:#?}");
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
            log::logln!("RESIZE\n{viewport:#?}");
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
        log::logln!("=> {}", unsafe { resp.num });
    }

    resp
}
