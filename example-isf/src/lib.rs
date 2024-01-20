use std::{
    ffi::CString,
    fmt::{Debug, Formatter},
    mem::transmute,
};
mod fullscreen_shader;
mod meta;
mod shader;
mod texture;
mod util;
use once_cell::sync::Lazy;

use ffgl_glium::{
    ffgl_handler,
    ffi::{ffgl1::PluginInfoStruct, ffgl2},
    logln,
    parameters::BasicParamInfo,
    plugin_info, FFGLGlium, FFGLGliumHandler, ParamHandler, ParamInfo, PluginType,
};
use glium::{
    backend::Facade,
    program::Uniform,
    uniforms::{AsUniformValue, EmptyUniforms, UniformValue, Uniforms},
    Texture2d,
};
use isf::{Input, Isf};
use rand::{rngs::StdRng, Rng, RngCore, SeedableRng};
use rand_seeder::Seeder;
use serde::de::Error;
use util::MultiUniforms;

#[derive(Debug, Clone)]
pub struct IsfState {
    pub info: Isf,
    pub inputs: Vec<IsfInputParam>,
    pub plugin_info: PluginInfoStruct,
}

impl Uniforms for IsfState {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut output: F) {
        for input in &self.inputs {
            let uniform = input.as_uniform_optional();
            if let Some(uniform) = uniform {
                output(&input.name, uniform);
            }
        }
    }
}

static INSTANCE: Lazy<IsfState> = Lazy::new(|| IsfState::new());

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));

#[derive(Debug, Clone)]
enum IsfInputValue {
    Event(bool),
    Bool(bool),
    Long(i32),
    Float(f32),
    Point2d([f32; 2]),
    Color([f32; 4]),
    None,
}

impl IsfInputValue {
    fn set(&mut self, index: usize, value: f32) {
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

    fn get(&self, index: usize) -> f32 {
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
struct IsfInputParam {
    ty: isf::InputType,
    name: String,
    params: Vec<BasicParamInfo>,
    value: IsfInputValue,
}

fn slice_from_vec(input: &Vec<f32>) -> [f32; 4] {
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
    fn new(Input { ty, name }: isf::Input) -> Self {
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

impl IsfState {
    fn new() -> Self {
        let info = isf::parse(ISF_SOURCE).unwrap();
        let params: Vec<IsfInputParam> = info
            .inputs
            .iter()
            .cloned()
            .map(|input| IsfInputParam::new(input))
            .collect();

        let mut name = [0; 16];
        let name_from_env = env!("ISF_NAME").as_bytes();

        name[0..name_from_env.len()].copy_from_slice(&name_from_env);

        let mut rng = Seeder::from(ISF_SOURCE).make_rng::<StdRng>();
        let mut code = [0; 4];
        rng.fill_bytes(&mut code);

        let plugin_type = if params.iter().any(|x| x.ty == isf::InputType::Image) {
            PluginType::Effect
        } else {
            PluginType::Source
        };

        let plugin_info = plugin_info(&code, &name, plugin_type);

        logln!("ISF INFO: {info:?}");
        logln!("ISF PARAMS: {params:?}");

        Self {
            info,
            inputs: params,
            plugin_info,
        }
    }

    fn param_info(&self, mut index: usize) -> &BasicParamInfo {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &self.inputs[input_index];
        let param = &input.params[index];

        param
    }

    fn get_param(&self, mut index: usize) -> f32 {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &self.inputs[input_index];

        input.value.get(index)
    }

    fn set_param(&mut self, mut index: usize, value: f32) {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &mut self.inputs[input_index];

        input.value.set(index, value);
    }
}

pub struct IsfFFGLInstance {
    pub shader: shader::IsfShader,
    pub state: IsfState,
}

impl Debug for IsfFFGLInstance {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("IsfFFGLInstance").finish()
    }
}

impl ParamHandler for IsfFFGLInstance {
    type Param = BasicParamInfo;

    fn num_params() -> usize {
        INSTANCE.inputs.iter().map(|x| x.params.len()).sum()
    }

    fn param_info(index: usize) -> &'static Self::Param {
        let param = INSTANCE.param_info(index);
        param
    }

    fn get_param(&self, index: usize) -> f32 {
        self.state.get_param(index)
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.state.set_param(index, value)
    }
}

impl FFGLGliumHandler for IsfFFGLInstance {
    fn info() -> &'static ffgl_glium::ffi::ffgl1::PluginInfoStruct {
        &INSTANCE.plugin_info
    }

    fn new(inst_data: &ffgl_glium::FFGLData, ctx: std::rc::Rc<glium::backend::Context>) -> Self {
        ffgl_glium::logln!("CREATED SHADER");

        let shader = shader::IsfShader::new(&ctx, &INSTANCE.info, ISF_SOURCE).unwrap();

        Self {
            shader,
            state: INSTANCE.clone(),
        }
    }

    fn render_frame(
        &mut self,
        target: &mut impl glium::Surface,
        textures: Vec<Texture2d>,
        inst_data: &ffgl_glium::FFGLData,
    ) -> Result<(), Box<dyn std::error::Error>> {
        let image_uniforms = self
            .state
            .inputs
            .iter()
            .filter_map(|i| {
                if let isf::InputType::Image = i.ty {
                    Some((
                        i.name.as_str(),
                        UniformValue::Texture2d(textures.first()?, None),
                    ))
                } else {
                    None
                }
            })
            .collect();

        let uniforms = MultiUniforms {
            uniforms: image_uniforms,
            next: &self.state,
        };

        self.shader.draw(target, &uniforms)?;

        Ok(())
    }
}

ffgl_handler!(FFGLGlium<IsfFFGLInstance>);
