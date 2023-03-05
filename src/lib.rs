mod conversions;
mod ffi;
mod instance;
mod nenum_derive;
mod log;
mod test_gl;
mod renderer;

use log::logln;
use test_gl::TestGl;

use std::{
    ffi::{c_void}, mem::transmute,
};

use ffi::*;

use conversions::*;
use instance::FFGLData;

type Instance = (FFGLData,TestGl);

#[no_mangle]
#[allow(non_snake_case)]
#[allow(unused_variables)]
pub extern "C" fn plugMain(
    functionCode: u32,
    inputValue: FFGLVal,
    instanceID: *mut Instance,
) -> FFGLVal {
    match Op::try_from(functionCode) {
        Ok(function) => {
            log::logln!("Op::{function:?}");
            plug_main(function, inputValue, unsafe{ instanceID.as_mut() })
        },
        Err(err) => {
            let err_text = format!("Received fnCode {functionCode:?}");
            logln!("Failed to parse fnCode {functionCode}");
            panic!();
        },
    }
}

#[derive(PartialEq)]
pub enum FFGLVersion {
    V1_5,
    V2_1
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

const fn chars<const N: usize>(input: &[u8;N]) -> &[i8;N] {
    unsafe { transmute::<&[u8;N], &[i8;N]>(input) } 
}

static mut INFO: ffgl::PluginInfoStruct = ffgl::PluginInfoStruct {
    APIMajorVersion: FFGL_VERSION.major(),
    APIMinorVersion: FFGL_VERSION.minor(),
    PluginUniqueID: *b"TRP0",
    PluginName: *b"testrustplugin  ",
    PluginType: ffgl::FF_SOURCE,
};

static mut INFO_EXTENDED: ffgl::PluginExtendedInfoStruct = ffgl::PluginExtendedInfoStruct {
    PluginMajorVersion: 0,
    PluginMinorVersion: 0,
    Description: chars(b"Built with Rust\0").as_ptr().cast_mut(),
    About: chars(b"Edward Taylor\0").as_ptr().cast_mut(),
    FreeFrameExtendedDataSize: 0,
    FreeFrameExtendedDataBlock: std::ptr::null::<c_void>() as *mut c_void,
};

fn get_max_coords(tex: &ffgl::FFGLTextureStruct) -> (f32, f32) {
    let s = (tex.Width as f32) / tex.HardwareWidth as f32;
    let t = (tex.Height as f32) / tex.HardwareHeight as f32;

    (s, t)
}

fn plug_main(function: Op, inputValue: FFGLVal, instance: Option<&mut Instance>) -> FFGLVal {
    match function {
        Op::FF_GETPLUGINCAPS => {
            let cap_num = unsafe { inputValue.num };
            logln!("Parsing CAP{cap_num}");
            let cap = PluginCapacity::try_from(cap_num).expect("Unexpected cap value{cap_num}");
            logln!("Cap: {cap:?}");

            match cap {
                PluginCapacity::FF_CAP_MINIMUMINPUTFRAMES => FFGLVal { num: 0 },
                PluginCapacity::FF_CAP_MAXIMUMINPUTFRAMES => FFGLVal { num: 0 },

                PluginCapacity::FF_CAP_PROCESSOPENGL => SupportVal::FF_SUPPORTED.into(),
                PluginCapacity::FF_CAP_SETTIME => SupportVal::FF_SUPPORTED.into(),

                _ => SupportVal::FF_UNSUPPORTED.into(),
            }
        }

        Op::FF_GETNUMPARAMETERS => FFGLVal { num: 0 },

        Op::FF_GETINFO => unsafe { FFGLVal::from_static_mut(&mut INFO) },

        Op::FF_GETEXTENDEDINFO => unsafe {
            FFGLVal::from_static_mut(&mut INFO_EXTENDED)
        },

        Op::FF_INSTANTIATEGL => {
            let viewport: &ffgl::FFGLViewportStruct = unsafe { inputValue.as_ref() };

            let new_data = FFGLData::new(viewport);
            let new_renderer = unsafe { TestGl::new() };

            log::logln!("INSTGL\n{new_data:?} Renderer\n{new_renderer:?}");

            FFGLVal::from_static_mut(Box::leak(Box::<Instance>::new((new_data, new_renderer))))
        }

        Op::FF_DEINSTANTIATEGL => {
            let inst = instance.unwrap();

            log::logln!("Deallocating instance {inst:?}");
            unsafe {
                drop(Box::from_raw(inst as *mut Instance));
            }

            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_PROCESSOPENGL => {
            let gl_process_info: &ffgl::ProcessOpenGLStruct = unsafe { inputValue.as_ref() };
            let (data,renderer) = instance.unwrap();

            unsafe {
                gl::ClearColor(data.host_beat.barPhase, data.host_beat.barPhase*3.123, 0.0, 1.0);
                gl::Clear(gl::COLOR_BUFFER_BIT);

                renderer.draw();

                // gl::
            }

            log::logln!("ProcessGL with struct\n{gl_process_info:#?} and\n{data:#?}");

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


        Op::FF_CONNECT => {
            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_INSTANTIATE
        | Op::FF_DEINSTANTIATE
        | Op::FF_PROCESSFRAME
        | Op::FF_PROCESSFRAMECOPY => SuccessVal::FF_FAIL.into(),

        Op::FF_INITIALISE_V2 => SuccessVal::FF_SUCCESS.into(),
        Op::FF_INITIALISE => {
            if FFGL_VERSION == FFGLVersion::V2_1 {
                SuccessVal::FF_FAIL.into()
            } else {
                SuccessVal::FF_SUCCESS.into()
            }
        }
        Op::FF_DEINITIALISE => SuccessVal::FF_SUCCESS.into(),

        _ => SuccessVal::FF_FAIL.into()
    }
}
