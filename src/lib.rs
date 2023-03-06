mod ffgl_derive;
mod instance;
mod nenum_derive;

pub mod ffi;
pub mod conversions;
pub mod log;
pub use instance::FFGLData;

// use log::logln;

use std::{ffi::c_void, mem::transmute};
use ffi::*;

pub use ffi::ffgl::ProcessOpenGLStruct;
pub use conversions::*;
pub use log::{loading_logger, FFGLLogger};
pub type Instance<T> = (FFGLData, T);


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

pub const fn plugin_info(uniqueId: &[u8;4], name: &[u8;16]) -> ffgl::PluginInfoStruct {
    ffgl::PluginInfoStruct {
        APIMajorVersion: FFGL_VERSION.major(),
        APIMinorVersion: FFGL_VERSION.minor(),
        PluginUniqueID: *uniqueId,
        PluginName: *name,
        PluginType: ffgl::FF_SOURCE,
    }
}

pub const fn plugin_info_extended(about: &'static str, description: &'static str) -> ffgl::PluginExtendedInfoStruct {
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

pub trait FFGLHandler: std::fmt::Debug {
    unsafe fn info() -> &'static mut ffgl::PluginInfoStruct {
        static mut INFO: ffgl::PluginInfoStruct = plugin_info(b"TRP0", b"testrustplugin  ");
        &mut INFO
    }

    unsafe fn info_extended() -> &'static mut ffgl::PluginExtendedInfoStruct {
        static mut INFO_EXTENDED: ffgl::PluginExtendedInfoStruct = plugin_info_extended("Edward Taylor\0", "Built with Rust\0");
        &mut INFO_EXTENDED
    }

    unsafe fn new(inst_data: &FFGLData) -> Self;
    unsafe fn draw(&mut self, inst_data: &FFGLData, frame_data: &ffgl::ProcessOpenGLStruct);
}

pub fn default_ffgl_callback<T: FFGLHandler + 'static>(
    function: Op,
    inputValue: FFGLVal,
    instance: Option<&mut Instance<T>>
) -> FFGLVal {
    match function {
        Op::FF_GETPLUGINCAPS => {
            let cap_num = unsafe { inputValue.num };
            log::logln!("Parsing CAP{cap_num}");
            let cap = PluginCapacity::try_from(cap_num).expect("Unexpected cap value{cap_num}");
            log::logln!("Cap: {cap:?}");

            match cap {
                PluginCapacity::FF_CAP_MINIMUMINPUTFRAMES => FFGLVal { num: 0 },
                PluginCapacity::FF_CAP_MAXIMUMINPUTFRAMES => FFGLVal { num: 0 },

                PluginCapacity::FF_CAP_PROCESSOPENGL => SupportVal::FF_SUPPORTED.into(),
                PluginCapacity::FF_CAP_SETTIME => SupportVal::FF_SUPPORTED.into(),

                _ => SupportVal::FF_UNSUPPORTED.into(),
            }
        }

        Op::FF_GETNUMPARAMETERS => FFGLVal { num: 0 },

        Op::FF_GETINFO => unsafe { FFGLVal::from_static_mut(T::info()) },

        Op::FF_GETEXTENDEDINFO => unsafe { FFGLVal::from_static_mut(T::info_extended()) },

        Op::FF_INSTANTIATEGL => {
            let viewport: &ffgl::FFGLViewportStruct = unsafe { inputValue.as_ref() };

            let new_data = FFGLData::new(viewport);
            let new_renderer = unsafe { T::new(&new_data) };

            log::logln!("INSTGL\n{new_data:?} Renderer\n{new_renderer:?}");

            FFGLVal::from_static_mut(Box::leak(Box::<Instance<T>>::new((new_data, new_renderer))))
        }

        Op::FF_DEINSTANTIATEGL => {
            let inst = instance.unwrap();

            log::logln!("Deallocating instance {inst:?}");
            unsafe {
                drop(Box::from_raw(inst as *mut Instance<T>));
            }

            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_PROCESSOPENGL => {
            let gl_process_info: &ffgl::ProcessOpenGLStruct = unsafe { inputValue.as_ref() };
            let (data, renderer) = instance.unwrap();

            unsafe {
                renderer.draw(&data, &gl_process_info);
            }

            log::logln!("ProcessGL with struct\n{gl_process_info:#?}\n{data:#?}\n{renderer:#?}");

            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_SETTIME => {
            let seconds: f64 = *unsafe { inputValue.as_ref() };
            instance.unwrap().0.set_time(seconds);
            SuccessVal::FF_SUCCESS.into()
        }

        //This is called before GLInitialize
        Op::FF_SET_BEATINFO => {
            let beat_info: &ffgl2::SetBeatinfoStruct = unsafe { inputValue.as_ref() };
            if let Some(instance) = instance {
                instance.0.set_beat(*beat_info);
            }
            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_CONNECT => SuccessVal::FF_SUCCESS.into(),

        Op::FF_INSTANTIATE
        | Op::FF_DEINSTANTIATE
        | Op::FF_PROCESSFRAME
        | Op::FF_PROCESSFRAMECOPY => SuccessVal::FF_FAIL.into(),

        Op::FF_INITIALISE_V2 => SuccessVal::FF_SUCCESS.into(),
        Op::FF_INITIALISE => SuccessVal::FF_SUCCESS.into(),
        Op::FF_DEINITIALISE => SuccessVal::FF_SUCCESS.into(),

        _ => SuccessVal::FF_FAIL.into(),
    }
}
