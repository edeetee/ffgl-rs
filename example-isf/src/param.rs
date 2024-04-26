use isf::{Input, InputLong, InputType, InputValues};

use glium::uniforms::UniformValue;

use ffgl_core::parameters::{
    builtin::OverlayParams,
    handler::{ParamInfoHandler, ParamValueHandler},
    ParamInfo, ParameterTypes, SimpleParamInfo,
};

use isf;

use std::{
    ffi::{CStr, CString},
    mem::transmute,
};

fn map_input_values<T, R>(in_values: InputValues<T>, map: &impl Fn(&T) -> R) -> InputValues<R> {
    InputValues {
        default: in_values.default.as_ref().map(map),
        min: in_values.default.as_ref().map(map),
        max: in_values.default.as_ref().map(map),
        identity: in_values.default.as_ref().map(map),
    }
}

///Holds ISF info along with the current value
#[derive(Debug, Clone)]
pub struct IsfValueAndInfo<T> {
    pub value: T,
    pub info: InputValues<T>,
}

impl<T: Default + Clone> IsfValueAndInfo<T> {
    pub fn new(info: InputValues<T>) -> Self {
        let value = info
            .default
            .clone()
            .unwrap_or(info.identity.clone().unwrap_or_default());

        Self { value, info }
    }
}

///Holds the value and the metadata about the value
#[derive(Debug, Clone)]
pub enum IsfInputValue {
    Event(bool),
    Bool(bool),
    Long(i32),
    Float(f32),
    Point2d([f32; 2]),
    Color([f32; 4]),
    None,
}

impl IsfInputValue {
    pub fn new(ty: InputType) -> Self {
        match ty {
            InputType::Event => Self::Event(false),
            InputType::Bool(x) => Self::Bool(x.default.unwrap_or_default()),
            InputType::Long(x) => Self::Long(x.default.unwrap_or_default()),
            InputType::Float(x) => Self::Float(x.default.or(x.identity).unwrap_or_default()),
            InputType::Point2d(x) => Self::Point2d(x.default.or(x.identity).unwrap_or_default()),
            InputType::Color(x) => Self::Color(slice_from_vec(
                &x.default.or(x.identity).unwrap_or_default(),
            )),
            InputType::Image => Self::None,
            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        }
    }
}

#[derive(Debug, Clone)]
pub struct IsfInputInfo(pub isf::Input);

impl ParamInfoHandler for IsfInputInfo {
    fn num_params(&self) -> usize {
        let ty = &self.0.ty;
        match ty {
            InputType::Event => 1,
            InputType::Bool(..) => 1,
            InputType::Long(..) => 1,
            InputType::Float(..) => 1,
            InputType::Point2d(..) => 2,
            InputType::Color(..) => 4,
            // isf::InputType::Image => 1,
            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        }
    }

    fn param_info(&self, index: usize) -> &dyn ParamInfo {
        let ty = &self.0.ty;
        let name = self.0.name;

        match ty {
            InputType::Point2d(x) => {
                let p_name = CString::new(match index {
                    0 => format!("{} x", name),
                    1 => format!("{} y", name),
                    _ => unreachable!("Index out of bounds for Point2d input type"),
                })
                .expect("Failed to create CString");

                let param_type = match index {
                    0 => ParameterTypes::X,
                    1 => ParameterTypes::Y,
                    _ => unreachable!(),
                };

                &SimpleParamInfo {
                    name: p_name,
                    param_type,
                    default: x.default.map(|x| x[index] as f32),
                    min: x.min.map(|x| x[index] as f32),
                    max: x.max.map(|x| x[index] as f32),
                    group: Some(name),
                }
            }
            InputType::Color(x) => {
                let p_name = CString::new(match index {
                    0 => format!("{} r", name),
                    1 => format!("{} g", name),
                    2 => format!("{} b", name),
                    3 => format!("{} a", name),
                    _ => unreachable!("Index out of bounds for Color input type"),
                })
                .expect("Failed to create CString");

                let param_type = match index {
                    0 => ParameterTypes::Red,
                    1 => ParameterTypes::Green,
                    2 => ParameterTypes::Blue,
                    3 => ParameterTypes::Alpha,
                    _ => unreachable!(),
                };

                &SimpleParamInfo {
                    name: p_name,
                    param_type,
                    default: x.default.map(|x| x[index] as f32),
                    min: x.min.map(|x| x[index] as f32),
                    max: x.max.map(|x| x[index] as f32),
                    group: Some(name),
                }
            }
            _ => {
                let name = CString::new(self.0.name.clone()).unwrap();

                match ty {
                    InputType::Event => &SimpleParamInfo {
                        name,
                        param_type: ParameterTypes::Event,
                        default: Some(false as u32 as f32),
                        min: None,
                        max: None,
                        group: None,
                    },
                    InputType::Bool(x) => &SimpleParamInfo {
                        name,
                        param_type: ParameterTypes::Boolean,
                        default: Some(x.default.unwrap_or_default() as u32 as f32),
                        min: None,
                        max: None,
                        group: None,
                    },
                    InputType::Long(x) => &SimpleParamInfo {
                        name,
                        param_type: ParameterTypes::Integer,
                        default: x
                            .default
                            .or_else(|| x.values.iter().next().copied())
                            .map(|x| x as f32),
                        min: x
                            .min
                            .or_else(|| x.values.iter().min().copied())
                            .map(|x| x as f32),
                        max: x
                            .max
                            .or_else(|| x.values.iter().max().copied())
                            .map(|x| x as f32),
                        group: None,
                    },

                    _ => unimplemented!("Unsupported ISF input type {ty:?}"),
                }
            }
        }
    }
}

#[derive(Debug, Clone)]
pub enum IsfFFGLParam {
    Isf(IsfShaderParam),
    Overlay(OverlayParams, f32),
}

impl ParamInfoHandler for IsfFFGLParam {
    fn param_info(&self, index: usize) -> &dyn ParamInfo {
        match self {
            Self::Isf(x) => x.info.param_info(index),
            Self::Overlay(x, _) => x,
        }
    }

    fn num_params(&self) -> usize {
        match self {
            Self::Isf(x) => x.info.num_params(),
            Self::Overlay(_, _) => 1,
        }
    }
}

impl ParamValueHandler for IsfFFGLParam {
    fn set(&mut self, index: usize, value: f32) {
        match self {
            Self::Isf(x) => x.value.set(index, value),
            Self::Overlay(_, x) => *x = value,
        }
    }

    fn get(&self, index: usize) -> f32 {
        match self {
            Self::Isf(x) => x.value.get(index),
            Self::Overlay(_, x) => *x,
        }
    }
}

#[derive(Debug, Clone)]
pub struct IsfShaderParam {
    pub info: IsfInputInfo,
    pub value: IsfInputValue,
}

pub fn slice_from_vec(input: &Vec<f32>) -> [f32; 4] {
    let mut slice = [0.0; 4];
    for (i, v) in input.iter().enumerate() {
        slice[i] = *v;
    }
    slice
}

pub trait AsUniformOptional {
    fn as_uniform_optional(&self) -> Option<UniformValue<'_>>;
}

impl AsUniformOptional for IsfShaderParam {
    fn as_uniform_optional(&self) -> Option<UniformValue<'_>> {
        let value = &self.value;

        match value {
            IsfInputValue::Event(x) => Some(UniformValue::Bool(*x)),
            IsfInputValue::Bool(x) => Some(UniformValue::Bool(*x)),
            IsfInputValue::Long(x, _) => Some(UniformValue::SignedInt(*x)),
            IsfInputValue::Float(x) => Some(UniformValue::Float(x.value)),
            IsfInputValue::Point2d(x) => Some(UniformValue::Vec2(x.value)),
            IsfInputValue::Color(x) => Some(UniformValue::Vec4(x.value)),
            IsfInputValue::None => None,
        }
    }
}

impl IsfShaderParam {
    pub fn new(isf_input: isf::Input) -> Self {
        let value = IsfInputValue::new(isf_input.ty.clone());
        let info = IsfInputInfo(isf_input);

        Self { info, value }
    }
}
