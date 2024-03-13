use isf::{Input, InputValues};

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

#[derive(Debug, Clone)]
pub enum IsfInputValue {
    Event(bool),
    Bool(bool),
    Long(IsfValueAndInfo<i32>),
    Float(IsfValueAndInfo<f32>),
    Point2d(IsfValueAndInfo<[f32; 2]>),
    Color(IsfValueAndInfo<[f32; 4]>),
    None,
}

impl IsfInputValue {
    pub fn new(ty: isf::InputType) -> Self {
        match ty {
            isf::InputType::Event => Self::Event(false),
            isf::InputType::Bool(x) => Self::Bool(x.default.unwrap_or_default()),
            isf::InputType::Long(x) => Self::Long(IsfValueAndInfo::new(x.input_values)),
            isf::InputType::Float(x) => Self::Float(IsfValueAndInfo::new(x)),
            isf::InputType::Point2d(x) => Self::Point2d(IsfValueAndInfo::new(x)),
            isf::InputType::Color(x) => {
                Self::Color(IsfValueAndInfo::new(map_input_values(x, &|v| {
                    slice_from_vec(&v)
                })))
            }
            isf::InputType::Image => Self::None,
            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        }
    }

    pub fn build_param_info(
        &self,
        index: usize,
        name: CString,
        param_type: ParameterTypes,
        group: Option<String>,
    ) -> SimpleParamInfo {
        let default = self.default(index);
        let min = self.min(index);
        let max = self.max(index);

        SimpleParamInfo {
            name,
            param_type,
            default,
            min,
            max,
            group,
        }
    }

    pub fn default(&self, index: usize) -> Option<f32> {
        match self {
            Self::Event(x) => Some(*x as u32 as f32),
            Self::Bool(x) => Some(*x as u32 as f32),
            Self::Long(x) => Some(unsafe { transmute(x.info.default.unwrap_or_default()) }),
            Self::Float(x) => x.info.default,
            Self::Point2d(x) => x.info.default.map(|arr| arr[index]),
            Self::Color(x) => x.info.default.map(|arr| arr[index]),
            Self::None => None,
        }
    }

    pub fn min(&self, index: usize) -> Option<f32> {
        match self {
            Self::Event(x) => Some(*x as u32 as f32),
            Self::Bool(x) => Some(*x as u32 as f32),
            Self::Long(x) => Some(unsafe { transmute(x.info.min.unwrap_or_default()) }),
            Self::Float(x) => x.info.min,
            Self::Point2d(x) => x.info.min.map(|arr| arr[index]),
            Self::Color(x) => x.info.min.map(|arr| arr[index]),
            Self::None => None,
        }
    }

    pub fn max(&self, index: usize) -> Option<f32> {
        match self {
            Self::Event(x) => Some(*x as u32 as f32),
            Self::Bool(x) => Some(*x as u32 as f32),
            Self::Long(x) => Some(unsafe { transmute(x.info.max.unwrap_or_default()) }),
            Self::Float(x) => x.info.max,
            Self::Point2d(x) => x.info.max.map(|arr| arr[index]),
            Self::Color(x) => x.info.max.map(|arr| arr[index]),
            Self::None => None,
        }
    }

    pub fn set(&mut self, index: usize, value: f32) {
        match self {
            Self::Event(x) => *x = value == 1.0,
            Self::Bool(x) => *x = value == 1.0,
            Self::Long(x) => x.value = unsafe { transmute(value) },
            Self::Float(x) => x.value = value,
            Self::Point2d(x) => x.value[index] = value,
            Self::Color(x) => x.value[index] = value,
            Self::None => {}
        }
    }

    pub fn get(&self, index: usize) -> f32 {
        match self {
            Self::Event(x) => *x as u32 as f32,
            Self::Bool(x) => *x as u32 as f32,
            Self::Long(x) => unsafe { transmute(x.value) },
            Self::Float(x) => x.value,
            Self::Point2d(x) => x.value[index],
            Self::Color(x) => x.value[index],
            Self::None => 0.0,
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
            Self::Isf(x) => &x.params[0],
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
    pub ty: isf::InputType,
    pub name: String,
    pub params: Vec<SimpleParamInfo>,
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
            IsfInputValue::Long(x) => Some(UniformValue::SignedInt(x.value)),
            IsfInputValue::Float(x) => Some(UniformValue::Float(x.value)),
            IsfInputValue::Point2d(x) => Some(UniformValue::Vec2(x.value)),
            IsfInputValue::Color(x) => Some(UniformValue::Vec4(x.value)),
            IsfInputValue::None => None,
        }
    }
}

impl IsfShaderParam {
    pub fn new(Input { ty, name }: isf::Input) -> Self {
        let value = IsfInputValue::new(ty.clone());

        let params = match &ty {
            isf::InputType::Event => vec![value.build_param_info(
                0,
                CString::new(name.clone()).unwrap(),
                ffgl_core::parameters::ParameterTypes::Event,
                None,
            )],
            isf::InputType::Bool(..) => vec![value.build_param_info(
                0,
                CString::new(name.clone()).unwrap(),
                ffgl_core::parameters::ParameterTypes::Boolean,
                None,
            )],
            isf::InputType::Long(..) => vec![value.build_param_info(
                0,
                CString::new(name.clone()).unwrap(),
                ffgl_core::parameters::ParameterTypes::Integer,
                None,
            )],
            isf::InputType::Float(..) => vec![value.build_param_info(
                0,
                CString::new(name.clone()).unwrap(),
                ffgl_core::parameters::ParameterTypes::Standard,
                None,
            )],
            isf::InputType::Point2d(..) => vec![
                value.build_param_info(
                    0,
                    CString::new(format!("{name} x")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::X,
                    Some(name.clone()),
                ),
                value.build_param_info(
                    1,
                    CString::new(format!("{name} y")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::Y,
                    Some(name.clone()),
                ),
            ],
            isf::InputType::Color(..) => vec![
                value.build_param_info(
                    0,
                    CString::new(format!("{name} r")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::Red,
                    Some(name.clone()),
                ),
                value.build_param_info(
                    1,
                    CString::new(format!("{name} g")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::Green,
                    Some(name.clone()),
                ),
                value.build_param_info(
                    2,
                    CString::new(format!("{name} b")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::Blue,
                    Some(name.clone()),
                ),
                value.build_param_info(
                    3,
                    CString::new(format!("{name} a")).unwrap(),
                    ffgl_core::parameters::ParameterTypes::Alpha,
                    Some(name.clone()),
                ),
            ],

            isf::InputType::Image => vec![],

            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        };

        Self {
            ty,
            params,
            name,
            value,
        }
    }
}
