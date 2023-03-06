// #![allow(non_snake_case)]
#![allow(non_camel_case_types)]

use std::any::Any;
use std::ffi::c_void;

use crate::ffgl::*;
use crate::ffgl2::{
    FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION, 
    FF_CONNECT, 
    FF_DISCONNECT,

    FF_INITIALISE_V2,
    FF_GET_PLUGIN_SHORT_NAME,
    FF_SET_BEATINFO,
    FF_SET_HOSTINFO,
    FF_SET_SAMPLERATE,
    FF_GET_THUMBNAIL,
    FF_RESIZE,
    FF_GET_PARAMETER_EVENTS,
};

use crate::nenum_derive::enum_const;

enum_const!(
    u32,
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

//FFGL
        FF_PROCESSOPENGL,
        FF_INSTANTIATEGL,
        FF_DEINSTANTIATEGL,
        FF_SETTIME,
        FF_INITIALISE_V2,

//FFGL2
        FF_GET_PLUGIN_SHORT_NAME,
        FF_SET_BEATINFO,
        FF_SET_HOSTINFO,
        FF_SET_SAMPLERATE,
        FF_GET_THUMBNAIL,
        FF_CONNECT,
        FF_DISCONNECT,
        FF_RESIZE,
        FF_GET_PARAMETER_EVENTS
    }
);

// const FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION: u32 = 16;
// mod ffgl2;

enum_const!(
    u32,
    pub enum PluginCapacity {
        // Plugin capabilities
        FF_CAP_16BITVIDEO,
        FF_CAP_24BITVIDEO,
        FF_CAP_32BITVIDEO,
        FF_CAP_PROCESSFRAMECOPY,
        FF_CAP_MINIMUMINPUTFRAMES,
        FF_CAP_MAXIMUMINPUTFRAMES,
        FF_CAP_COPYORINPLACE,
        
        // Plugin optimization
        // FF_CAP_PREFER_NONE,
        // FF_CAP_PREFER_INPLACE,
        // FF_CAP_PREFER_COPY,
        // FF_CAP_PREFER_BOTH,
        
        // new plugin capabilities for FFGL
        FF_CAP_SETTIME,
        FF_CAP_PROCESSOPENGL,
        FF_CAP_TOP_LEFT_TEXTURE_ORIENTATION
    }
);

enum_const!(
    u32,
    pub enum SuccessVal {
        FF_SUCCESS,
        FF_FAIL,
    }
);

enum_const!(
    u32,
    pub enum BoolVal {
        FF_TRUE,
        FF_FALSE
    }
);

enum_const!(
    u32,
    pub enum SupportVal {
        FF_SUPPORTED,
        FF_UNSUPPORTED,
    }
);


#[repr(C)]
pub union FFGLVal {
    pub num: u32,
    ptr: *const c_void
}

impl FFGLVal {
    ///Only use for const variables that will stick around
    pub fn from_static_mut(a: &'static mut impl Any) -> Self {
        Self {
            ptr: a as *const _ as *const c_void
        }
    }

    pub unsafe fn as_ref<T>(&self) -> &T {
        &*(self.ptr as *const T)
    }
}

impl Into<FFGLVal> for SuccessVal {
    fn into(self) -> FFGLVal {
        FFGLVal{num: self.into()}
    }
}
impl Into<FFGLVal> for BoolVal {
    fn into(self) -> FFGLVal {
        FFGLVal{num: self.into()}
    }
}
impl Into<FFGLVal> for SupportVal {
    fn into(self) -> FFGLVal {
        FFGLVal{num: self.into()}
    }
}