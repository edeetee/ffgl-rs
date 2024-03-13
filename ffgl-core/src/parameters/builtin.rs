use std::ffi::CStr;

use super::info::{ParamInfo, ParameterTypes};

#[derive(Debug, Clone, Copy)]
#[repr(C)]
pub enum OverlayParams {
    Scale,
}

impl ParamInfo for OverlayParams {
    fn name(&self) -> &CStr {
        &CStr::from_bytes_with_nul(match self {
            Self::Scale => b"Resize\0",
        })
        .unwrap()
    }

    fn param_type(&self) -> ParameterTypes {
        ParameterTypes::Standard
    }

    fn min(&self) -> f32 {
        0.0
    }

    fn max(&self) -> f32 {
        1.0
    }

    fn default_val(&self) -> f32 {
        1.0
    }

    fn group(&self) -> &str {
        "opts"
    }
}
