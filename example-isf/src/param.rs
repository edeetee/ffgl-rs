use isf::Input;

use glium::uniforms::UniformValue;

use ffgl_glium::parameters::BasicParamInfo;

use isf;

use std::{ffi::CString, mem::transmute};

#[derive(Debug, Clone)]
pub(crate) enum IsfInputValue {
    Event(bool),
    Bool(bool),
    Long(i32),
    Float(f32),
    Point2d([f32; 2]),
    Color([f32; 4]),
    None,
}

impl IsfInputValue {
    pub(crate) fn set(&mut self, index: usize, value: f32) {
        match self {
            Self::Event(x) => *x = value == 1.0,
            Self::Bool(x) => *x = value == 1.0,
            Self::Long(x) => *x = unsafe { transmute(value) },
            Self::Float(x) => *x = value,
            Self::Point2d(x) => x[index] = value,
            Self::Color(x) => x[index] = value,
            Self::None => {}
        }
    }

    pub(crate) fn get(&self, index: usize) -> f32 {
        match self {
            Self::Event(x) => *x as u32 as f32,
            Self::Bool(x) => *x as u32 as f32,
            Self::Long(x) => unsafe { transmute(*x) },
            Self::Float(x) => *x,
            Self::Point2d(x) => x[index],
            Self::Color(x) => x[index],
            Self::None => 0.0,
        }
    }
}

#[derive(Debug, Clone)]
pub(crate) struct IsfInputParam {
    pub(crate) ty: isf::InputType,
    pub(crate) name: String,
    pub(crate) params: Vec<BasicParamInfo>,
    pub(crate) value: IsfInputValue,
}

pub(crate) fn slice_from_vec(input: &Vec<f32>) -> [f32; 4] {
    let mut slice = [0.0; 4];
    for (i, v) in input.iter().enumerate() {
        slice[i] = *v;
    }
    slice
}

pub trait AsUniformOptional {
    fn as_uniform_optional(&self) -> Option<UniformValue<'_>>;
}

impl AsUniformOptional for IsfInputParam {
    fn as_uniform_optional(&self) -> Option<UniformValue<'_>> {
        let ty = &self.ty;
        let value = &self.value;

        match (ty, value) {
            (isf::InputType::Event, IsfInputValue::Event(x)) => Some(UniformValue::Bool(*x)),
            (isf::InputType::Bool(_), IsfInputValue::Bool(x)) => Some(UniformValue::Bool(*x)),
            (isf::InputType::Long(_), IsfInputValue::Long(x)) => Some(UniformValue::SignedInt(*x)),
            (isf::InputType::Float(_), IsfInputValue::Float(x)) => Some(UniformValue::Float(*x)),
            (isf::InputType::Point2d(_), IsfInputValue::Point2d(x)) => {
                Some(UniformValue::Vec2([x[0], x[1]]))
            }

            (isf::InputType::Color(_), IsfInputValue::Color(x)) => {
                Some(UniformValue::Vec4([x[0], x[1], x[2], x[3]]))
            }
            (isf::InputType::Image, IsfInputValue::None) => None,

            _ => panic!("Invalid uniform value for ISF input {ty:?}\n val {value:?}"),
        }
    }
}

impl IsfInputParam {
    pub(crate) fn new(Input { ty, name }: isf::Input) -> Self {
        let value = match &ty {
            isf::InputType::Event => IsfInputValue::Event(false),
            isf::InputType::Bool(x) => IsfInputValue::Bool(x.default.unwrap_or_default()),
            isf::InputType::Long(x) => IsfInputValue::Long(x.default.unwrap_or_default()),
            isf::InputType::Float(x) => IsfInputValue::Float(x.default.unwrap_or_default()),
            isf::InputType::Point2d(x) => IsfInputValue::Point2d(x.default.unwrap_or_default()),
            isf::InputType::Color(x) => {
                IsfInputValue::Color(x.default.as_ref().map(slice_from_vec).unwrap_or_default())
            }
            isf::InputType::Image => IsfInputValue::None,

            _ => unimplemented!("Unsupported ISF input type {ty:?}"),
        };

        let params = match &ty {
            isf::InputType::Event => vec![BasicParamInfo {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Event,
                default: Some(value.get(0)),
                ..Default::default()
            }],
            isf::InputType::Bool(..) => vec![BasicParamInfo {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Boolean,
                default: Some(value.get(0)),
                ..Default::default()
            }],
            isf::InputType::Long(..) => vec![BasicParamInfo {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Integer,
                default: Some(value.get(0)),
                ..Default::default()
            }],
            isf::InputType::Float(..) => vec![BasicParamInfo {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Standard,
                default: Some(value.get(0)),
                ..Default::default()
            }],
            isf::InputType::Point2d(..) => vec![
                BasicParamInfo {
                    name: CString::new(format!("{name} x")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::X,
                    group: Some(name.clone()),
                    default: Some(value.get(0)),
                    ..Default::default()
                },
                BasicParamInfo {
                    name: CString::new(format!("{name} y")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Y,
                    group: Some(name.clone()),
                    default: Some(value.get(1)),
                    ..Default::default()
                },
            ],
            isf::InputType::Color(..) => vec![
                BasicParamInfo {
                    name: CString::new(format!("{name} r")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Red,
                    group: Some(name.clone()),
                    default: Some(value.get(0)),
                    ..Default::default()
                },
                BasicParamInfo {
                    name: CString::new(format!("{name} g")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Green,
                    group: Some(name.clone()),
                    default: Some(value.get(1)),
                    ..Default::default()
                },
                BasicParamInfo {
                    name: CString::new(format!("{name} b")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Blue,
                    group: Some(name.clone()),
                    default: Some(value.get(2)),
                    ..Default::default()
                },
                BasicParamInfo {
                    name: CString::new(format!("{name} a")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Alpha,
                    group: Some(name.clone()),
                    default: Some(value.get(3)),
                    ..Default::default()
                },
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
