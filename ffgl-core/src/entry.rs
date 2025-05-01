//! Primary entry point of the FFGL plugin. This is the function that is called by the host.
//! You can use [crate::plugin_main] to automate calling this entry function from the FFGL ABI
//!
use crate::ffi::util::copy_str_to_host_buffer;
use crate::info;
use crate::FFGLData;

use crate::ffi::ffgl2::*;

use crate::handler::{FFGLHandler, FFGLInstance};
use crate::log::try_init_default_subscriber;
use crate::parameters::ParamInfo;

use std::sync::OnceLock;
use std::{any::Any, ffi::CString};

use crate::conversions::*;

use crate::handler;
use anyhow::{Context, Error};

fn param<H: FFGLHandler>(handler: &'static H, index: FFGLVal) -> &'static dyn ParamInfo {
    handler.param_info(unsafe { index.num as usize })
}

static INFO: OnceLock<info::PluginInfo> = OnceLock::new();
static INFO_STRUCT: OnceLock<PluginInfoStruct> = OnceLock::new();
static ABOUT: OnceLock<CString> = OnceLock::new();
static DESCRIPTION: OnceLock<CString> = OnceLock::new();

static mut INFO_STRUCT_EXTENDED: Option<PluginExtendedInfoStruct> = None;

static HANDLER: OnceLock<Box<dyn Any + Send + Sync>> = OnceLock::new();

use tracing::debug_span;
use tracing::trace_span;
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
    let handler = HANDLER
        .get_or_init(|| {
            let _ = try_init_default_subscriber();
            Box::new(H::init())
        })
        .downcast_ref::<H>()
        .context(e!("Handler type mismatch"))?;

    // Initialize plugin info if not already initialized
    let info = INFO.get_or_init(|| handler.plugin_info());

    // Initialize about and description strings
    let about =
        ABOUT.get_or_init(|| CString::new(info.about.clone()).expect("Invalid about string"));
    let description = DESCRIPTION.get_or_init(|| {
        CString::new(info.description.clone()).expect("Invalid description string")
    });

    // Initialize info structs
    let _info_struct = INFO_STRUCT.get_or_init(|| {
        unsafe {
            INFO_STRUCT_EXTENDED = Some(info::plugin_info_extended(about, description));
        }
        info::plugin_info(
            unsafe { std::mem::transmute(&info.unique_id) },
            unsafe { std::mem::transmute(&info.name) },
            info.ty,
        )
    });

    let name = std::str::from_utf8(&info.name).unwrap_or_default();

    let _span = if !function.is_noisy() {
        debug_span!("entry", "fn" = ?function, name, "in" = unsafe { input_value.num })
    } else {
        trace_span!("entry", "fn" = ?function, name, "in" = unsafe { input_value.num })
    }
    .entered();

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

            debug!(r = unsafe { result.num }, "{cap:?}");

            result
        }

        Op::EnablePluginCap => {
            let cap_num = unsafe { input_value.num };
            let cap = num::FromPrimitive::from_u32(cap_num).expect("Unexpected cap n{cap_num}");

            let result: FFGLVal = match cap {
                PluginCapacity::TopLeftTextureOrientation => SuccessVal::Success.into(),
                _ => SuccessVal::Fail.into(),
            };

            debug!(r = unsafe { result.num }, "{cap:?}");

            result
        }

        Op::GetNumParameters => FFGLVal {
            num: handler.num_params() as u32,
        },

        Op::GetParameterDefault => param(handler, input_value).default_val().into(),
        Op::GetParameterGroup => {
            let input: &GetStringStruct = unsafe { input_value.as_ref() };
            let buffer = input.stringBuffer;

            let group = H::param_info(handler, input.parameterNumber as usize).group();

            unsafe {
                copy_str_to_host_buffer(
                    buffer.address as *mut u8,
                    buffer.maxToWrite as usize,
                    group,
                )
            };

            debug!(g = group);

            SuccessVal::Success.into()
        }
        Op::GetParameterDisplayName => {
            let input: &GetStringStruct = unsafe { input_value.as_ref() };
            let buffer = input.stringBuffer;

            let display_name =
                H::param_info(handler, input.parameterNumber as usize).display_name();

            unsafe {
                copy_str_to_host_buffer(
                    buffer.address as *mut u8,
                    buffer.maxToWrite as usize,
                    display_name,
                )
            };

            debug!(d = display_name);

            SuccessVal::Success.into()
        }
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

        Op::GetNumParameterElements => {
            let index = unsafe { input_value.num } as usize;
            (handler.param_info(index).num_elements() as u32).into()
        }

        Op::GetParameterElementName => {
            let input: &mut GetParameterElementNameStruct = unsafe { input_value.as_mut() };

            let param_index = input.ParameterNumber;
            let elm_index = input.ElementNumber;

            handler
                .param_info(param_index as usize)
                .element_name(elm_index as usize)
                .into()
        }

        Op::GetParameterElementValue => {
            let input: &mut GetParameterElementValueStruct = unsafe { input_value.as_mut() };

            let param_index = input.ParameterNumber;
            let elm_index = input.ElementNumber;

            handler
                .param_info(param_index as usize)
                .element_value(elm_index as usize)
                .into()
        }

        Op::GetNumElementSeparators => (0 as u32).into(),

        Op::GetInfo => INFO_STRUCT.get().context(e!("No info"))?.into(),

        Op::GetExtendedInfo => (&raw mut INFO_STRUCT_EXTENDED).into(),

        Op::InstantiateGL => {
            let viewport: &FFGLViewportStruct = unsafe { input_value.as_ref() };

            let data = FFGLData::new(viewport);
            let renderer = H::new_instance(handler, &data)
                .context("Failed to instantiate renderer")
                .context(format!("For {}", std::str::from_utf8(&info.name).unwrap()))?;

            let instance = handler::Instance { data, renderer };

            info!(
                id = ?info.unique_id,
                name = ?String::from_utf8_lossy(&info.name),
                "Created INSTANCE:\n{instance:#?}",
            );

            FFGLVal::from_static(Box::leak(Box::<handler::Instance<H::Instance>>::new(
                instance,
            )))
        }

        Op::DeinstantiateGL => {
            let inst = instance.context(e!("No instance"))?;

            debug!(?inst, "DEINSTGL");
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

            let handler::Instance { data, .. } = instance.context(e!("No instance"))?;
            data.viewport = *viewport;

            debug!(v = ?viewport, "RESIZE");
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

    if !function.is_noisy() {
        debug!(r = unsafe { resp.num }, "DONE");
    } else {
        trace!(r = unsafe { resp.num }, "DONE");
    }

    Ok(resp)
}
