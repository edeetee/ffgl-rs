#![allow(non_camel_case_types)]

use crate::ffgl2::*;
use num_derive::{FromPrimitive, ToPrimitive};

#[repr(u32)]
#[derive(FromPrimitive, Debug)]
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
#[derive(Debug)]
pub enum InputStatus {
    NotInUse = FF_INPUT_NOTINUSE,
    InUse = FF_INPUT_INUSE,
}

#[repr(u32)]
#[derive(Debug)]
pub enum ParameterUsages {
    Standard = FF_USAGE_STANDARD,
    FFT = FF_USAGE_FFT,
}

#[repr(u64)]
#[derive(Debug)]
pub enum ParameterEventFlags {
    Visibility = FF_EVENT_FLAG_VISIBILITY,
    DisplayName = FF_EVENT_FLAG_DISPLAY_NAME,
    Value = FF_EVENT_FLAG_VALUE,
    Elements = FF_EVENT_FLAG_ELEMENTS,
}
