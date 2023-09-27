pub mod conversions;
mod ffgl_derive;
pub mod ffi;
mod instance;
pub mod log;
pub mod parameters;
pub use instance::FFGLData;
pub mod validate;

use ffi::*;
use std::{ffi::c_void, fmt::Debug, mem::transmute};

pub use conversions::*;
pub use ffi::ffgl::*;
pub use log::{loading_logger, FFGLLogger};

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
    V1_5,
    V2_1,
}

impl FFGLVersion {
    const fn major(&self) -> u32 {
        match self {
            FFGLVersion::V1_5 => 1,
            FFGLVersion::V2_1 => 2,
        }
    }

    const fn minor(&self) -> u32 {
        match self {
            FFGLVersion::V1_5 => 5,
            FFGLVersion::V2_1 => 1,
        }
    }
}

const FFGL_VERSION: FFGLVersion = FFGLVersion::V2_1;

const fn chars<const N: usize>(input: &[u8; N]) -> &[i8; N] {
    unsafe { transmute::<&[u8; N], &[i8; N]>(input) }
}

// static mut INFO: ffgl::PluginInfoStruct = plugin_info(b"TRP0", b"testrustplugin  ");
// static mut INFO_EXTENDED: ffgl::PluginExtendedInfoStruct =

pub const fn plugin_info(uniqueId: &[u8; 4], name: &[u8; 16]) -> ffgl::PluginInfoStruct {
    ffgl::PluginInfoStruct {
        APIMajorVersion: FFGL_VERSION.major(),
        APIMinorVersion: FFGL_VERSION.minor(),
        PluginUniqueID: *uniqueId,
        PluginName: *name,
        PluginType: ffgl::FF_SOURCE,
    }
}

pub const fn plugin_info_extended(
    about: &'static str,
    description: &'static str,
) -> ffgl::PluginExtendedInfoStruct {
    ffgl::PluginExtendedInfoStruct {
        PluginMajorVersion: 0,
        PluginMinorVersion: 0,
        Description: about.as_ptr().cast_mut().cast(),
        About: description.as_ptr().cast_mut().cast(),
        FreeFrameExtendedDataSize: 0,
        FreeFrameExtendedDataBlock: std::ptr::null::<c_void>() as *mut c_void,
    }
}

fn get_max_coords(tex: &ffgl::FFGLTextureStruct) -> (f32, f32) {
    let s = (tex.Width as f32) / tex.HardwareWidth as f32;
    let t = (tex.Height as f32) / tex.HardwareHeight as f32;

    (s, t)
}

pub trait FFGLHandler: Debug {
    unsafe fn info() -> &'static mut ffgl::PluginInfoStruct {
        static mut INFO: ffgl::PluginInfoStruct = plugin_info(b"TRP0", b"testrustplugin  ");
        &mut INFO
    }

    unsafe fn info_extended() -> &'static mut ffgl::PluginExtendedInfoStruct {
        static mut INFO_EXTENDED: ffgl::PluginExtendedInfoStruct =
            plugin_info_extended("Edward Taylor\0", "Built with Rust\0");
        &mut INFO_EXTENDED
    }

    ///Called by [Op::FF_INSTANTIATEGL] to create a new instance of the plugin
    unsafe fn new(inst_data: &FFGLData) -> Self;
    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: &ffgl::ProcessOpenGLStruct);
}

pub fn default_ffgl_callback<T: FFGLHandler + 'static>(
    function: Op,
    inputValue: FFGLVal,
    instance: Option<&mut Instance<T>>,
) -> FFGLVal {
    match function {
        Op::ProcessOpenGL | Op::SetBeatInfo | Op::SetTime | Op::GetParameterEvents => {}
        _ => {
            log::logln!("Op::{function:?}({})", unsafe { inputValue.num });
        }
    }

    match function {
        Op::GetPluginCaps => {
            let cap_num = unsafe { inputValue.num };
            let cap = num::FromPrimitive::from_u32(cap_num).expect("Unexpected cap n{cap_num}");

            let result = match cap {
                PluginCapacity::MinInputFrames => FFGLVal { num: 0 },
                PluginCapacity::MaxInputFrames => FFGLVal { num: 0 },

                PluginCapacity::ProcessOpenGl => SupportVal::Supported.into(),
                PluginCapacity::SetTime => SupportVal::Supported.into(),

                PluginCapacity::TopLeftTextureOrientation => SupportVal::Unsupported.into(),

                _ => SupportVal::Unsupported.into(),
            };

            log::logln!("{cap:?} => {}", unsafe { result.num });

            result
        }

        Op::GetNumParameters => FFGLVal { num: 0 },

        Op::GetInfo => unsafe { FFGLVal::from_static_mut(T::info()) },

        Op::GetExtendedInfo => unsafe { FFGLVal::from_static_mut(T::info_extended()) },

        Op::InstantiateGL => {
            let viewport: &ffgl::FFGLViewportStruct = unsafe { inputValue.as_ref() };

            let data = FFGLData::new(viewport);
            let renderer = unsafe { T::new(&data) };
            let instance = Instance { data, renderer };

            log::logln!("INSTGL\n{instance:#?}");

            FFGLVal::from_static_mut(Box::leak(Box::<Instance<T>>::new(instance)))
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
            let gl_process_info: &ffgl::ProcessOpenGLStruct = unsafe { inputValue.as_ref() };
            let Instance { data, renderer } = instance.unwrap();

            unsafe {
                // validate::validate_context_state();
                renderer.draw(&data, &gl_process_info);
                // validate::validate_context_state();
            }

            SuccessVal::Success.into()
        }

        Op::SetTime => {
            let seconds: f64 = *unsafe { inputValue.as_ref() };
            instance.unwrap().data.set_time(seconds);
            SuccessVal::Success.into()
        }

        //This is called before GLInitialize
        Op::SetBeatInfo => {
            let beat_info: &ffgl2::SetBeatinfoStruct = unsafe { inputValue.as_ref() };
            if let Some(instance) = instance {
                instance.data.set_beat(*beat_info);
            }
            SuccessVal::Success.into()
        }

        Op::Resize => {
            let viewport: &ffgl::FFGLViewportStruct = unsafe { inputValue.as_ref() };
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
    }
}
