#![allow(non_camel_case_types)]

use crate::{ffgl2::*, FFGLVal};
use num_derive::{FromPrimitive, ToPrimitive};

#[repr(u32)]
#[derive(FromPrimitive, Debug, Clone, Copy)]
pub enum ParameterTypes {
    Boolean = FF_TYPE_BOOLEAN,
    Event = FF_TYPE_EVENT,
    Red = FF_TYPE_RED,
    Green = FF_TYPE_GREEN,
    Blue = FF_TYPE_BLUE,
    X = FF_TYPE_XPOS,
    Y = FF_TYPE_YPOS,
    Standard = FF_TYPE_STANDARD,
    Option = FF_TYPE_OPTION,
    Buffer = FF_TYPE_BUFFER,
    Integer = FF_TYPE_INTEGER,
    File = FF_TYPE_FILE,
    Text = FF_TYPE_TEXT,
    Hue = FF_TYPE_HUE,
    Saturation = FF_TYPE_SATURATION,
    Brightness = FF_TYPE_BRIGHTNESS,
    Alpha = FF_TYPE_ALPHA,
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

pub struct Param {
    pub name: &'static str,
    pub display_name: &'static str,
    pub usage: ParameterUsages,
    pub value: ParamValue,
}

impl Default for Param {
    fn default() -> Self {
        Param {
            name: "UNKNOWN_NAME",
            display_name: "UNKNOWN_DISPLAY",
            usage: ParameterUsages::Standard,
            value: ParamValue::Standard(1.0),
        }
    }
}

impl Param {
    pub fn default(&self) -> ParamValue {
        Into::<ParameterTypes>::into(self.value).into()
    }
}

#[derive(Debug, Clone, Copy)]
pub enum ParamValue {
    Boolean(bool),
    Event,
    Red(f32),
    Green(f32),
    Blue(f32),
    X(f32),
    Y(f32),
    Standard(f32),
    Option(u32),
    Buffer,
    Integer(i32),
    File,
    Text,
    Hue(f32),
    Saturation(f32),
    Brightness(f32),
    Alpha(f32),
}

impl From<ParamValue> for FFGLVal {
    fn from(value: ParamValue) -> Self {
        match value {
            ParamValue::Boolean(b) => FFGLVal { num: b as u32 },
            ParamValue::Event => FFGLVal { num: 0 },
            ParamValue::Red(f) => FFGLVal { num: f as u32 },
            ParamValue::Green(f) => FFGLVal { num: f as u32 },
            ParamValue::Blue(f) => FFGLVal { num: f as u32 },
            ParamValue::X(f) => FFGLVal { num: f as u32 },
            ParamValue::Y(f) => FFGLVal { num: f as u32 },
            ParamValue::Standard(f) => FFGLVal { num: f as u32 },
            ParamValue::Option(u) => FFGLVal { num: u },
            ParamValue::Buffer => FFGLVal { num: 0 },
            ParamValue::Integer(i) => FFGLVal { num: i as u32 },
            ParamValue::File => FFGLVal { num: 0 },
            ParamValue::Text => FFGLVal { num: 0 },
            ParamValue::Hue(f) => FFGLVal { num: f as u32 },
            ParamValue::Saturation(f) => FFGLVal { num: f as u32 },
            ParamValue::Brightness(f) => FFGLVal { num: f as u32 },
            ParamValue::Alpha(f) => FFGLVal { num: f as u32 },
        }
    }
}

impl From<ParamValue> for ParameterTypes {
    fn from(value: ParamValue) -> Self {
        match value {
            ParamValue::Boolean(_) => ParameterTypes::Boolean,
            ParamValue::Event => ParameterTypes::Event,
            ParamValue::Red(_) => ParameterTypes::Red,
            ParamValue::Green(_) => ParameterTypes::Green,
            ParamValue::Blue(_) => ParameterTypes::Blue,
            ParamValue::X(_) => ParameterTypes::X,
            ParamValue::Y(_) => ParameterTypes::Y,
            ParamValue::Standard(_) => ParameterTypes::Standard,
            ParamValue::Option(_) => ParameterTypes::Option,
            ParamValue::Buffer => ParameterTypes::Buffer,
            ParamValue::Integer(_) => ParameterTypes::Integer,
            ParamValue::File => ParameterTypes::File,
            ParamValue::Text => ParameterTypes::Text,
            ParamValue::Hue(_) => ParameterTypes::Hue,
            ParamValue::Saturation(_) => ParameterTypes::Saturation,
            ParamValue::Brightness(_) => ParameterTypes::Brightness,
            ParamValue::Alpha(_) => ParameterTypes::Alpha,
        }
    }
}

impl From<ParameterTypes> for ParamValue {
    fn from(value: ParameterTypes) -> Self {
        match value {
            ParameterTypes::Boolean => ParamValue::Boolean(false),
            ParameterTypes::Event => ParamValue::Event,
            ParameterTypes::Red => ParamValue::Red(1.0),
            ParameterTypes::Green => ParamValue::Green(1.0),
            ParameterTypes::Blue => ParamValue::Blue(1.0),
            ParameterTypes::Alpha => ParamValue::Alpha(1.0),
            ParameterTypes::X => ParamValue::X(0.0),
            ParameterTypes::Y => ParamValue::Y(0.0),
            ParameterTypes::Standard => ParamValue::Standard(0.0),
            ParameterTypes::Option => ParamValue::Option(0),
            ParameterTypes::Buffer => ParamValue::Buffer,
            ParameterTypes::Integer => ParamValue::Integer(0),
            ParameterTypes::File => ParamValue::File,
            ParameterTypes::Text => ParamValue::Text,
            ParameterTypes::Hue => ParamValue::Hue(0.0),
            ParameterTypes::Saturation => ParamValue::Saturation(0.0),
            ParameterTypes::Brightness => ParamValue::Brightness(1.0),
        }
    }
}
