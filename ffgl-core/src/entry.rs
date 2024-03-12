//! Primary entry point of the FFGL plugin. This is the function that is called by the host.
//! You can use [crate::ffgl_handler] to automate calling this entry function from the FFGL ABI
//!
use crate::info;
use crate::instance::FFGLData;

use crate::ffi::ffgl2::*;

use crate::handler::{FFGLHandler, FFGLInstance};
use crate::log::try_init_default_subscriber;
use crate::parameters::ParamInfo;

use core::slice;

use std::{any::Any, ffi::CString};

use crate::conversions::*;

use crate::handler;
use anyhow::{Context, Error};

fn param<H: FFGLHandler>(handler: &'static H, index: FFGLVal) -> &'static H::Param {
    handler.param_info(unsafe { index.num as usize })
}

static mut INITIALIZED: bool = false;
static mut INFO: Option<info::PluginInfo> = None;
static mut INFO_STRUCT: Option<PluginInfoStruct> = None;
static mut ABOUT: Option<CString> = None;
static mut DESCRIPTION: Option<CString> = None;
static mut INFO_STRUCT_EXTENDED: Option<PluginExtendedInfoStruct> = None;
static mut HANDLER: Option<Box<dyn Any>> = None;

use tracing::{debug, info, trace};

///backtrace didn't seem to work. Maybe a problem with FFI. This is a hacky way to get the source
macro_rules! e {
    ($($arg:tt)*) => {{
        format!("{orig}\nSOURCE {file}:{line}:{column}", orig=format!($($arg)*),
        file = file!(),
        line = line!(),
        column = column!(),)
    }}
}

pub fn default_ffgl_entry<H: FFGLHandler + 'static>(
    function: Op,
    mut input_value: FFGLVal,
    instance: Option<&mut handler::Instance<H::Instance>>,
) -> Result<FFGLVal, Error> {
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
        debug!("Op::{function:?}({})", unsafe { input_value.num });
    } else {
        trace!("Op::{function:?}({})", unsafe { input_value.num });
    }

    unsafe {
        if !INITIALIZED {
            INITIALIZED = true;

            HANDLER = Some(Box::new(H::init()));

            let _ = try_init_default_subscriber();

            let handler = &*HANDLER
                .as_ref()
                .context(e!("No handler"))?
                .downcast_ref::<H>()
                .context(e!("Handler incorrect type"))?;

            INFO = Some(handler.plugin_info());
            let info = INFO.as_ref().context(e!("No info"))?;
            ABOUT = Some(CString::new(info.about.clone())?);
            DESCRIPTION = Some(CString::new(info.description.clone())?);

            INFO_STRUCT = Some(info::plugin_info(
                std::mem::transmute(&info.unique_id),
                std::mem::transmute(&info.name),
                info.ty,
            ));

            INFO_STRUCT_EXTENDED = Some(info::plugin_info_extended(
                ABOUT.as_ref().context(e!("ABOUT not initialized"))?,
                DESCRIPTION
                    .as_ref()
                    .context(e!("DESCRIPTION not initialized"))?,
            ));

            info!(
                "INITIALIZED PLUGIN '{id:?}' '{name}'",
                name = std::str::from_utf8(&info.name)?,
                id = info.unique_id
            );
        }
    }

    let info = unsafe { INFO.as_ref().context(e!("No info"))? };

    let handler = unsafe { &HANDLER }
        .as_ref()
        .context(e!("Handler not initialized"))?
        .downcast_ref::<H>()
        .context(e!("Handler type mismatch"))?;

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

        Op::GetNumParameters => FFGLVal {
            num: handler.num_params() as u32,
        },

        Op::GetParameterDefault => param(handler, input_value).default_val().into(),
        Op::GetParameterGroup => {
            let input: &GetStringStructTag = unsafe { input_value.as_ref() };
            let buffer = input.stringBuffer;

            let group = H::param_info(handler, input.parameterNumber as usize).group();

            let string_target: &mut [char] = unsafe {
                slice::from_raw_parts_mut(buffer.address as *mut char, buffer.maxToWrite as usize)
            };

            let copied_chars = std::cmp::min(group.len(), buffer.maxToWrite as usize);

            string_target[..copied_chars]
                .copy_from_slice(&group[..copied_chars].chars().collect::<Vec<_>>());

            debug!("GET PARAM GROUP {group:?}");

            SuccessVal::Success.into()
        }
        Op::GetParameterDisplay => param(handler, input_value).display_name().into(),
        Op::GetParameterName => param(handler, input_value).name().into(),
        Op::GetParameterType => param(handler, input_value).param_type().into(),

        Op::GetParameter => instance
            .context(e!("No instance"))?
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

            instance
                .context(e!("No instance"))?
                .renderer
                .set_param(index_usize, new_value);

            SuccessVal::Success.into()
        }
        Op::GetParameterRange => {
            let input: &mut GetRangeStruct = unsafe { (input_value).as_mut() };

            let index = input.parameterNumber;
            let param = handler.param_info(index as usize);

            input.range = RangeStruct {
                min: param.min(),
                max: param.max(),
            };

            SuccessVal::Success.into()
        }
        // Op::GetParameterGroup => param(instance, ffgl2::GetParameterGroupStruct).group.into(),
        Op::GetInfo => unsafe { INFO_STRUCT.as_ref().context(e!("No info"))?.into() },

        Op::GetExtendedInfo => unsafe {
            INFO_STRUCT_EXTENDED
                .as_ref()
                .context(e!("No extended info"))?
                .into()
        },

        Op::InstantiateGL => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };

            let data = FFGLData::new(viewport);
            let renderer = H::new_instance(handler, &data)
                .context("Failed to instantiate renderer")
                .context(format!("For {}", std::str::from_utf8(&info.name).unwrap()))?;

            let instance = handler::Instance { data, renderer };

            info!(
                "Created INSTANCE \n{instance:#?} of ({id:?}, '{name:?}')",
                id = info.unique_id,
                name = String::from_utf8_lossy(&info.name),
            );

            FFGLVal::from_static(Box::leak(Box::<handler::Instance<H::Instance>>::new(
                instance,
            )))
        }

        Op::DeinstantiateGL => {
            let inst = instance.context(e!("No instance"))?;

            debug!("DEINSTGL\n{inst:#?}");
            unsafe {
                drop(Box::from_raw(inst as *mut handler::Instance<H::Instance>));
            }

            SuccessVal::Success.into()
        }

        Op::ProcessOpenGL => {
            let gl_process_info: &ProcessOpenGLStruct = unsafe { input_value.as_ref() };

            let handler::Instance { data, renderer } = instance.context(e!("No instance"))?;
            let gl_input = gl_process_info.into();

            renderer.draw(&data, gl_input);

            SuccessVal::Success.into()
        }

        Op::SetTime => {
            let seconds: f64 = *unsafe { input_value.as_ref() };
            instance.context(e!("No instance"))?.data.set_time(seconds);
            SuccessVal::Success.into()
        }

        //This is can be called before GLInitialize.
        Op::SetBeatInfo => {
            let beat_info: &SetBeatinfoStruct = unsafe { input_value.as_ref() };
            if let Some(inst) = instance {
                inst.data.set_beat(*beat_info);
                SuccessVal::Success.into()
            } else {
                SuccessVal::Fail.into()
            }
        }

        Op::Resize => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };
            debug!("RESIZE\n{viewport:#?}");
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
        debug!("=> {}", unsafe { resp.num });
    } else {
        trace!("=> {}", unsafe { resp.num });
    }

    Ok(resp)
}
