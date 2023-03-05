#![feature(const_cstr_methods)]
// #![feature(strict_provenance)]

mod ffglrs;
mod ffgl;
mod instance;
mod nenum_derive;
mod gl;
mod log;
mod ffgl2;

use log::logln;

use std::{
    ffi::{c_void}, mem::transmute,
};

use ffglrs::*;
use instance::FFGLInstance;

#[no_mangle]
#[allow(non_snake_case)]
#[allow(unused_variables)]
pub extern "C" fn plugMain(
    functionCode: u32,
    inputValue: FFGLVal,
    instanceID: *mut FFGLInstance,
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

fn plug_main(function: Op, inputValue: FFGLVal, instance: Option<&mut FFGLInstance>) -> FFGLVal {
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

            let new_inst = FFGLInstance::new(viewport);
            log::logln!("INSTGL {new_inst:?} with viewport {viewport:?}");

            FFGLVal::from_static_mut(Box::leak(Box::new(new_inst)))
        }

        Op::FF_DEINSTANTIATEGL => {
            let inst = instance.unwrap();

            log::logln!("Deallocating instance {inst:?}");

            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_PROCESSOPENGL => {
            let gl_process_info: &ffgl::ProcessOpenGLStruct = unsafe { inputValue.as_ref() };
            let inst = instance.unwrap();

            unsafe {
                // gl::BindFramebuffer(gl::DRAW_FRAMEBUFFER, gl_process_info.HostFBO);
                // gl::Viewport(
                //     inst.viewport.x as i32,
                //     inst.viewport.y as i32,
                //     inst.viewport.width as i32,
                //     inst.viewport.height as i32,
                // );
                gl::ClearColor(inst.host_beat.barPhase, 0.0, 0.0, 1.0);
            }

            log::logln!("ProcessGL with struct\n{gl_process_info:#?} and\n{inst:#?}");

            SuccessVal::FF_SUCCESS.into()
        }

        Op::FF_SETTIME => {
            let seconds: f64 = *unsafe { inputValue.as_ref() };
            // log::logln!("Seconds: {seconds}");
            instance.unwrap().set_time(seconds);
            SuccessVal::FF_SUCCESS.into()
        }
        Op::FF_SET_BEATINFO => {
            let beat_info: &ffgl2::SetBeatinfoStruct = unsafe { inputValue.as_ref() };
            // logln!("Beat Info {beat_info:?}");
            if let Some(instance) = instance {
                instance.set_beat(*beat_info);
            }
            // instance.unwrap()
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
