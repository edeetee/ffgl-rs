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
    path::Iter,
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
    pub fn new(ty: &InputType) -> Self {
        match ty {
            InputType::Event => Self::Event(false),
            InputType::Bool(x) => Self::Bool(x.default.unwrap_or_default()),
            InputType::Long(x) => Self::Long(x.default.unwrap_or_default()),
            InputType::Float(x) => Self::Float(x.default.or(x.identity).unwrap_or_default()),
            InputType::Point2d(x) => Self::Point2d(x.default.or(x.identity).unwrap_or_default()),
            InputType::Color(x) => Self::Color(slice_from_vec(
                &x.default
                    .as_ref()
                    .or(x.identity.as_ref())
                    .cloned()
                    .unwrap_or_default(),
            )),
            InputType::Image => Self::None,
            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        }
    }

    pub fn set(&mut self, index: usize, value: f32) {
        match self {
            Self::Event(x) => *x = value == 1.0,
            Self::Bool(x) => *x = value == 1.0,
            Self::Long(x) => *x = value as i32,
            Self::Float(x) => *x = value,
            Self::Point2d(x) => x[index] = value,
            Self::Color(x) => x[index] = value,
            Self::None => {}
        }
    }

    pub fn get(&self, index: usize) -> f32 {
        match self {
            Self::Event(x) => *x as u32 as f32,
            Self::Bool(x) => *x as u32 as f32,
            Self::Long(x) => *x as f32,
            Self::Float(x) => *x,
            Self::Point2d(x) => x[index],
            Self::Color(x) => x[index],
            Self::None => 0.0,
        }
    }
}

fn param_info_for_isf_input(isf::Input { name, ty }: &isf::Input) -> Vec<SimpleParamInfo> {
    match ty {
        InputType::Point2d(x) => (0..2)
            .map(|i| {
                let p_name = CString::new(match i {
                    0 => format!("{} x", name),
                    1 => format!("{} y", name),
                    _ => unreachable!("Index out of bounds for Point2d input type"),
                })
                .expect("Failed to create CString");

                let param_type = match i {
                    0 => ParameterTypes::X,
                    1 => ParameterTypes::Y,
                    _ => unreachable!(),
                };

                SimpleParamInfo {
                    name: p_name,
                    param_type,
                    default: x.default.map(|x| x[i] as f32),
                    min: x.min.map(|x| x[i] as f32),
                    max: x.max.map(|x| x[i] as f32),
                    group: Some(name.clone()),
                }
            })
            .collect(),
        InputType::Color(x) => (0..4)
            .map(|i| {
                let p_name = CString::new(match i {
                    0 => format!("{} r", name),
                    1 => format!("{} g", name),
                    2 => format!("{} b", name),
                    3 => format!("{} a", name),
                    _ => unreachable!("Index out of bounds for Color input type"),
                })
                .expect("Failed to create CString");

                let param_type = match i {
                    0 => ParameterTypes::Red,
                    1 => ParameterTypes::Green,
                    2 => ParameterTypes::Blue,
                    3 => ParameterTypes::Alpha,
                    _ => unreachable!(),
                };

                SimpleParamInfo {
                    name: p_name,
                    param_type,
                    default: x.default.as_ref().map(|x| x[i] as f32),
                    min: x.min.as_ref().map(|x| x[i] as f32),
                    max: x.max.as_ref().map(|x| x[i] as f32),
                    group: Some(name.clone()),
                }
            })
            .collect(),

        InputType::Image => vec![],
        _ => {
            let name = CString::new(name.clone()).unwrap();

            vec![match ty {
                InputType::Event => SimpleParamInfo {
                    name,
                    param_type: ParameterTypes::Event,
                    default: Some(false as u32 as f32),
                    min: None,
                    max: None,
                    group: None,
                },
                InputType::Bool(x) => SimpleParamInfo {
                    name,
                    param_type: ParameterTypes::Boolean,
                    default: Some(x.default.unwrap_or_default() as u32 as f32),
                    min: None,
                    max: None,
                    group: None,
                },
                InputType::Float(x) => SimpleParamInfo {
                    name,
                    param_type: ParameterTypes::Standard,
                    default: x.default.or(x.identity),
                    min: x.min.or(x.identity),
                    max: x.max.or(x.identity),
                    group: None,
                },
                InputType::Long(x) => SimpleParamInfo {
                    name,
                    param_type: ParameterTypes::Integer,
                    default: x
                        .default
                        .or_else(|| x.values.first().copied())
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
            }]
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
            Self::Isf(x) => &x.params[index],
            Self::Overlay(x, _) => x,
        }
    }

    fn num_params(&self) -> usize {
        match self {
            Self::Isf(x) => x.params.len(),
            Self::Overlay(_, _) => 1,
        }
    }
}

impl ParamValueHandler for IsfFFGLParam {
    fn set_param(&mut self, index: usize, value: f32) {
        match self {
            Self::Isf(x) => x.value.set(index, value),
            Self::Overlay(_, x) => *x = value,
        }
    }

    fn get_param(&self, index: usize) -> f32 {
        match self {
            Self::Isf(x) => x.value.get(index),
            Self::Overlay(_, x) => *x,
        }
    }
}

#[derive(Debug, Clone)]
pub struct IsfShaderParam {
    pub value: IsfInputValue,
    pub name: String,
    pub ty: InputType,
    pub params: Vec<SimpleParamInfo>,
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
            IsfInputValue::Long(x) => Some(UniformValue::SignedInt(*x)),
            IsfInputValue::Float(x) => Some(UniformValue::Float(*x)),
            IsfInputValue::Point2d(x) => Some(UniformValue::Vec2(*x)),
            IsfInputValue::Color(x) => Some(UniformValue::Vec4(*x)),
            IsfInputValue::None => None,
        }
    }
}

impl IsfShaderParam {
    pub fn new(input: isf::Input) -> Self {
        let params = param_info_for_isf_input(&input);
        let isf::Input { name, ty } = input;
        let value = IsfInputValue::new(&ty);

        Self {
            value,
            name,
            ty,
            params,
        }
    }
}
