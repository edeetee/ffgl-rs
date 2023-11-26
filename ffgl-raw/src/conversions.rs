// #![allow(non_snake_case)]
#![allow(non_camel_case_types)]

use std::ffi::{c_void, CStr};

use num_derive::{FromPrimitive, ToPrimitive};

use crate::ffgl1::{
    FF_CAP_16BITVIDEO, FF_CAP_24BITVIDEO, FF_CAP_32BITVIDEO, FF_CAP_MINIMUMINPUTFRAMES,
    FF_CAP_PROCESSFRAMECOPY, FF_CAP_PROCESSOPENGL, FF_CAP_SETTIME, FF_GETINFO,
};
use crate::ffgl2::*;

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum Op {
    GetInfo = FF_GETINFO,
    Initialise,
    Deinitialise,
    ProcessFrame,
    GetNumParameters,
    GetParameterName,
    GetParameterDefault,
    GetParameterDisplay,
    SetParameter,
    GetParameter,
    GetPluginCaps,
    Instantiate,
    Deinstantiate,
    GetExtendedInfo,
    ProcessFrameCopy,
    GetParameterType,
    GetInputStatus,

    ProcessOpenGL,
    InstantiateGL,
    DeinstantiateGL,
    SetTime,
    Connect,
    Disconnect,
    Resize,
    InitialiseV2 = FF_INITIALISE_V2,

    // FFGL2
    GetPluginShortName = FF_GET_PLUGIN_SHORT_NAME,

    SetBeatInfo = FF_SET_BEATINFO,
    SetHostInfo,
    SetSampleRate,

    GetThumbnail = FF_GET_THUMBNAIL,

    GetParameterEvents = FF_GET_PARAMETER_EVENTS,

    GetParameterRange = FF_GET_RANGE,
    GetParameterVisibility = FF_GET_PRAMETER_VISIBILITY,
    GetParameterGroup = FF_GET_PARAM_GROUP,

    EnablePluginCap = FF_ENABLE_PLUGIN_CAP,
}

impl TryFrom<u32> for Op {
    type Error = ();

    fn try_from(value: u32) -> Result<Self, Self::Error> {
        num::FromPrimitive::from_u32(value).ok_or(())
    }
}

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum PluginCapacity {
    ///old
    Video16b = FF_CAP_16BITVIDEO,
    Video24 = FF_CAP_24BITVIDEO,
    Video32 = FF_CAP_32BITVIDEO,
    ProcessFrameCopy = FF_CAP_PROCESSFRAMECOPY,

    ProcessOpenGl = FF_CAP_PROCESSOPENGL,

    SetTime = FF_CAP_SETTIME,

    MinInputFrames = FF_CAP_MINIMUMINPUTFRAMES,
    MaxInputFrames = FF_CAP_MAXIMUM_INPUT_FRAMES,

    TopLeftTextureOrientation = FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION,
}

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum SuccessVal {
    Success = FF_SUCCESS,
    Fail = FF_FAIL,
}

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum BoolVal {
    True = FF_TRUE,
    False = FF_FALSE,
}

#[repr(u32)]
#[derive(FromPrimitive, ToPrimitive, Debug)]
pub enum SupportVal {
    Supported = FF_SUPPORTED,
    Unsupported = FF_UNSUPPORTED,
}

#[repr(C)]
pub union FFGLVal {
    pub num: u32,
    ptr: *const c_void,
}

impl std::fmt::Debug for FFGLVal {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("FFGLVal")
            .field("num", unsafe { &self.num })
            .finish()
    }
}

impl From<&'static CStr> for FFGLVal {
    fn from(a: &'static CStr) -> Self {
        Self {
            ptr: a.as_ptr() as *const c_void,
        }
    }
}

impl<T> From<&'static T> for FFGLVal {
    fn from(a: &'static T) -> Self {
        Self::from_static(a)
    }
}

impl<T> From<&'static mut T> for FFGLVal {
    fn from(a: &'static mut T) -> Self {
        Self::from_static(a)
    }
}

impl FFGLVal {
    ///Only use for const variables that will stick around
    pub fn from_static<T: ?Sized>(a: &'static T) -> Self {
        Self {
            ptr: a as *const _ as *const c_void,
        }
    }

    pub unsafe fn as_ref<T>(&self) -> &T {
        &*(self.ptr as *const T)
    }

    pub unsafe fn as_mut<T>(&mut self) -> &mut T {
        &mut *(self.ptr as *mut T)
    }
}

impl Into<FFGLVal> for SuccessVal {
    fn into(self) -> FFGLVal {
        FFGLVal { num: self as u32 }
    }
}
impl Into<FFGLVal> for BoolVal {
    fn into(self) -> FFGLVal {
        FFGLVal { num: self as u32 }
    }
}
impl Into<FFGLVal> for SupportVal {
    fn into(self) -> FFGLVal {
        FFGLVal { num: self as u32 }
    }
}
