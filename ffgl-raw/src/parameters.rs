#![allow(non_camel_case_types)]

use std::ffi::{CStr, CString};

use crate::{ffi::ffgl2::*, FFGLVal};
use num_derive::FromPrimitive;

#[repr(u32)]
#[derive(Default, FromPrimitive, Debug, Clone, Copy)]
pub enum ParameterTypes {
    Boolean = FF_TYPE_BOOLEAN,
    Event = FF_TYPE_EVENT,
    Red = FF_TYPE_RED,
    Green = FF_TYPE_GREEN,
    Blue = FF_TYPE_BLUE,
    X = FF_TYPE_XPOS,
    Y = FF_TYPE_YPOS,
    #[default]
    Standard = FF_TYPE_STANDARD,
    Option = FF_TYPE_OPTION,
    Buffer = FF_TYPE_BUFFER,
    Integer = FF_TYPE_INTEGER,
    Hue = FF_TYPE_HUE,
    Saturation = FF_TYPE_SATURATION,
    Brightness = FF_TYPE_BRIGHTNESS,
    Alpha = FF_TYPE_ALPHA,
}

impl From<ParameterTypes> for FFGLVal {
    fn from(value: ParameterTypes) -> Self {
        FFGLVal { num: value as u32 }
    }
}

impl ParameterTypes {
    pub fn default_value(&self) -> f32 {
        0.0
    }
}

#[repr(u32)]
#[derive(Debug, Clone, Copy)]
pub enum InputStatus {
    NotInUse = FF_INPUT_NOTINUSE,
    InUse = FF_INPUT_INUSE,
}

#[repr(u32)]
#[derive(Debug, Clone, Copy)]
pub enum ParameterUsages {
    Standard = FF_USAGE_STANDARD,
    FFT = FF_USAGE_FFT,
}

#[repr(u64)]
#[derive(Debug, Clone, Copy)]
pub enum ParameterEventFlags {
    Visibility = FF_EVENT_FLAG_VISIBILITY,
    DisplayName = FF_EVENT_FLAG_DISPLAY_NAME,
    Value = FF_EVENT_FLAG_VALUE,
    Elements = FF_EVENT_FLAG_ELEMENTS,
}

//Param as a trait
pub trait ParamInfo {
    fn name(&self) -> &CStr;
    fn display_name(&self) -> &CStr {
        self.name()
    }

    fn usage(&self) -> ParameterUsages {
        ParameterUsages::Standard
    }

    // fn get(&self) -> f32;

    // fn set(&mut self, value: f32);

    fn min(&self) -> f32 {
        0.0
    }

    fn max(&self) -> f32 {
        1.0
    }

    fn param_type(&self) -> ParameterTypes {
        ParameterTypes::Standard
    }

    fn default(&self) -> f32 {
        self.param_type().default_value()
    }

    fn group(&self) -> &str {
        ""
    }
}

pub trait ParamValue {
    fn get(&self) -> f32;
    fn set(&mut self, value: f32);
}

impl ParamValue for f32 {
    fn get(&self) -> f32 {
        *self
    }

    fn set(&mut self, value: f32) {
        *self = value;
    }
}

#[derive(Default, Debug, Clone)]
pub struct BasicParamInfo {
    pub name: CString,
    pub param_type: ParameterTypes,
    pub default: Option<f32>,
    pub min: Option<f32>,
    pub max: Option<f32>,
    pub group: Option<String>,
}

impl BasicParamInfo {
    pub fn from_name(name: &str) -> Self {
        let name = unsafe { CStr::from_bytes_with_nul_unchecked(name.as_bytes()) };

        BasicParamInfo {
            name: name.into(),
            param_type: ParameterTypes::Standard,
            ..Default::default()
        }
    }
}

impl ParamInfo for BasicParamInfo {
    fn name(&self) -> &CStr {
        &self.name
    }

    fn param_type(&self) -> ParameterTypes {
        self.param_type
    }

    // fn get(&self) -> f32 {
    //     self.param_value.unwrap_or(self.param_type.default_value())
    // }

    // fn set(&mut self, value: f32) {
    //     self.param_value = Some(value);
    // }

    fn min(&self) -> f32 {
        self.min.unwrap_or(0.0)
    }

    fn max(&self) -> f32 {
        self.max.unwrap_or(1.0)
    }

    fn default(&self) -> f32 {
        self.default.unwrap_or(self.param_type.default_value())
    }

    fn group(&self) -> &str {
        self.group.as_deref().unwrap_or("")
    }
}

// #[derive(Debug, Clone, Copy)]
// pub enum ParamValue {
//     Float(f32),
//     Bool(bool),
//     Int(u32),
// }

// impl ParamValue {
//     pub fn set(&mut self, value: f32) {
//         match self {
//             ParamValue::Float(f) => *f = value,
//             ParamValue::Bool(b) => *b = value > 0.0,
//             ParamValue::Int(i) => *i = value as u32,
//         }
//     }
// }

// impl From<ParamValue> for FFGLVal {
//     fn from(value: ParamValue) -> Self {
//         match value {
//             ParamValue::Float(f) => FFGLVal {
//                 num: unsafe { std::mem::transmute::<f32, u32>(f) },
//             },
//             ParamValue::Bool(b) => FFGLVal { num: b as u32 },
//             ParamValue::Int(i) => FFGLVal { num: i },
//         }
//     }
// }

// impl From<ParamValue> for ParameterTypes {
//     fn from(value: ParamValue) -> Self {
//         match value {
//             ParamValue::Boolean(_) => ParameterTypes::Boolean,
//             ParamValue::Event => ParameterTypes::Event,
//             ParamValue::Red(_) => ParameterTypes::Red,
//             ParamValue::Green(_) => ParameterTypes::Green,
//             ParamValue::Blue(_) => ParameterTypes::Blue,
//             ParamValue::X(_) => ParameterTypes::X,
//             ParamValue::Y(_) => ParameterTypes::Y,
//             ParamValue::Standard(_) => ParameterTypes::Standard,
//             ParamValue::Option(_) => ParameterTypes::Option,
//             ParamValue::Buffer => ParameterTypes::Buffer,
//             ParamValue::Integer(_) => ParameterTypes::Integer,
//             ParamValue::Brightness(_) => ParameterTypes::Brightness,
//             ParamValue::Alpha(_) => ParameterTypes::Alpha,
//             ParamValue::Hue(_) => ParameterTypes::Hue,
//             ParamValue::Saturation(_) => ParameterTypes::Saturation,
//         }
//     }
// }

// impl From<ParameterTypes> for ParamValue {
//     fn from(value: ParameterTypes) -> Self {
//         match value {
//             ParameterTypes::Boolean => todo!(),
//             ParameterTypes::Event => todo!(),
//             ParameterTypes::Red => ParamValue::Red(1.0),
//             ParameterTypes::Green => ParamValue::Green(1.0),
//             ParameterTypes::Blue => ParamValue::Blue(1.0),
//             ParameterTypes::Alpha => ParamValue::Alpha(1.0),
//             ParameterTypes::X => ParamValue::X(0.0),
//             ParameterTypes::Y => ParamValue::Y(0.0),
//             ParameterTypes::Standard => ParamValue::Standard(0.0),
//             ParameterTypes::Option => ParamValue::Option(0),
//             ParameterTypes::Buffer => ParamValue::Buffer,
//             ParameterTypes::Integer => ParamValue::Integer(0),
//             ParameterTypes::Hue => ParamValue::Hue(0.0),
//             ParameterTypes::Saturation => ParamValue::Saturation(0.0),
//             ParameterTypes::Brightness => ParamValue::Brightness(1.0),
//         }
//     }
// }
