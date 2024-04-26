#![allow(non_camel_case_types)]
use std::ffi::CString;

use std::ffi::CStr;

use crate::{conversions::FFGLVal, ffi::ffgl2::*};
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

    fn min(&self) -> f32 {
        0.0
    }

    fn max(&self) -> f32 {
        1.0
    }

    fn param_type(&self) -> ParameterTypes {
        ParameterTypes::Standard
    }

    fn element_name(&self, _index: usize) -> &CStr {
        self.name()
    }

    fn element_value(&self, _index: usize) -> f32 {
        self.default_val()
    }

    fn num_elements(&self) -> usize {
        1
    }

    fn default_val(&self) -> f32 {
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
pub struct SimpleParamInfo {
    pub name: CString,
    pub param_type: ParameterTypes,
    pub default: Option<f32>,
    pub min: Option<f32>,
    pub max: Option<f32>,
    pub group: Option<String>,
    pub elements: Option<Vec<(CString, f32)>>,
}

impl SimpleParamInfo {
    pub fn new(name: &str) -> Self {
        let name = CString::new(name).unwrap();

        SimpleParamInfo {
            name,
            ..Default::default()
        }
    }
}

impl ParamInfo for SimpleParamInfo {
    fn name(&self) -> &CStr {
        &self.name
    }

    fn param_type(&self) -> ParameterTypes {
        self.param_type
    }

    fn min(&self) -> f32 {
        self.min.unwrap_or(0.0)
    }

    fn max(&self) -> f32 {
        self.max.unwrap_or(1.0)
    }

    fn default_val(&self) -> f32 {
        self.default.unwrap_or(self.param_type.default_value())
    }

    fn group(&self) -> &str {
        self.group.as_deref().unwrap_or("")
    }

    fn element_name(&self, index: usize) -> &CStr {
        if let Some(elements) = &self.elements {
            &elements[index].0
        } else {
            self.name()
        }
    }

    fn element_value(&self, index: usize) -> f32 {
        if let Some(elements) = &self.elements {
            elements[index].1
        } else {
            self.default_val()
        }
    }

    fn num_elements(&self) -> usize {
        self.elements.as_ref().map(|x| x.len()).unwrap_or(1)
    }
}
