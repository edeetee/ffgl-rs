// #![allow(non_snake_case)]
#![allow(non_camel_case_types)]

use std::any::Any;
use std::ffi::c_void;

use num_derive::{FromPrimitive, ToPrimitive};

use crate::ffgl::*;
use crate::ffgl2::{
    FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION, FF_GET_PARAMETER_EVENTS, FF_GET_THUMBNAIL,
    FF_INITIALISE_V2,
};

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum Op {
    FF_GETINFO,
    FF_INITIALISE,
    FF_DEINITIALISE,
    FF_PROCESSFRAME,
    FF_GETNUMPARAMETERS,
    FF_GETPARAMETERNAME,
    FF_GETPARAMETERDEFAULT,
    FF_GETPARAMETERDISPLAY,
    FF_SETPARAMETER,
    FF_GETPARAMETER,
    FF_GETPLUGINCAPS,
    FF_INSTANTIATE,
    FF_DEINSTANTIATE,
    FF_GETEXTENDEDINFO,
    FF_PROCESSFRAMECOPY,
    FF_GETPARAMETERTYPE,
    FF_GETIPUTSTATUS,

    FF_PROCESSOPENGL,
    FF_INSTANTIATEGL,
    FF_DEINSTANTIATEGL,
    FF_SETTIME,
    FF_CONNECT,
    FF_DISCONNECT,
    FF_RESIZE,
    FF_INITIALISE_V2 = FF_INITIALISE_V2,

    //FFGL2
    FF_GET_PLUGIN_SHORT_NAME = 33,

    FF_SET_BEATINFO = 38,
    FF_SET_HOSTINFO,
    FF_SET_SAMPLERATE,

    FF_GET_THUMBNAIL = FF_GET_THUMBNAIL,

    FF_GET_PARAMETER_EVENTS = FF_GET_PARAMETER_EVENTS,
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
    ///DEPRECIATED
    FF_CAP_16BITVIDEO,
    FF_CAP_24BITVIDEO,
    FF_CAP_32BITVIDEO,
    FF_CAP_PROCESSFRAMECOPY,
    FF_CAP_PROCESSOPENGL,

    FF_CAP_SETTIME = FF_CAP_SETTIME,

    FF_CAP_MINIMUMINPUTFRAMES = FF_CAP_MINIMUMINPUTFRAMES,
    FF_CAP_MAXIMUMINPUTFRAMES,

    FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION = FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION,
}

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum SuccessVal {
    FF_SUCCESS,
    FF_FAIL = FF_FAIL,
}

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
pub enum BoolVal {
    FF_TRUE = FF_TRUE,
    FF_FALSE = FF_FALSE,
}

#[repr(u32)]
#[derive(FromPrimitive, ToPrimitive, Debug)]
pub enum SupportVal {
    FF_SUPPORTED = FF_SUPPORTED,
    FF_UNSUPPORTED = FF_UNSUPPORTED,
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

impl FFGLVal {
    ///Only use for const variables that will stick around
    pub fn from_static_mut(a: &'static mut impl Any) -> Self {
        Self {
            ptr: a as *const _ as *const c_void,
        }
    }

    pub unsafe fn as_ref<T>(&self) -> &T {
        &*(self.ptr as *const T)
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
