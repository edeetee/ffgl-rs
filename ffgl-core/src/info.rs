//! Use these to configure what the host expects from your plugin

use std::ffi::c_void;

use std::ffi::CStr;

use num_derive::FromPrimitive;
use num_derive::ToPrimitive;
use num_traits::ToPrimitive;

use crate::ffi::ffgl2::*;

#[repr(u32)]
#[derive(FromPrimitive, ToPrimitive, Debug, Clone, Copy, PartialEq)]
pub enum PluginType {
    Effect = FF_EFFECT,
    Source = FF_SOURCE,
    Mixer = FF_MIXER,
}

#[derive(PartialEq)]
pub enum FFGLVersion {
    // V1_5,
    V2_1,
}

impl FFGLVersion {
    pub const fn major(&self) -> u32 {
        match self {
            // FFGLVersion::V1_5 => 1,
            FFGLVersion::V2_1 => 2,
        }
    }

    pub const fn minor(&self) -> u32 {
        match self {
            // FFGLVersion::V1_5 => 5,
            FFGLVersion::V2_1 => 1,
        }
    }
}

pub(crate) const FFGL_VERSION_RESOLUME: FFGLVersion = FFGLVersion::V2_1;

#[derive(Debug, Clone)]
pub struct PluginInfo {
    pub unique_id: [u8; 4],
    pub name: [u8; 16],
    pub ty: PluginType,
    pub about: String,
    pub description: String,
}

pub fn plugin_info(
    unique_id: &[i8; 4],
    name: &[i8; 16],
    plugin_type: PluginType,
) -> PluginInfoStruct {
    PluginInfoStruct {
        APIMajorVersion: FFGL_VERSION_RESOLUME.major(),
        APIMinorVersion: FFGL_VERSION_RESOLUME.minor(),
        PluginUniqueID: *unique_id,
        PluginName: *name,
        PluginType: plugin_type.to_u32().unwrap(),
    }
}

pub const fn plugin_info_extended(
    about: &'static CStr,
    description: &'static CStr,
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
