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
    ffgl_handler, logln,
    parameters::{BasicParam, ParamValue},
    plugin_info, FFGLGlium, FFGLGliumHandler, Param, ParamHandler,
};
use glium::{
    program::Uniform,
    uniforms::{AsUniformValue, EmptyUniforms, UniformValue, Uniforms},
};
use isf::{Input, Isf};
use rand::{rngs::StdRng, Rng, RngCore, SeedableRng};
use rand_seeder::Seeder;

#[derive(Debug, Clone)]
pub struct IsfState {
    pub info: Isf,
    pub inputs: Vec<IsfInputParam>,
}

impl Uniforms for IsfState {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut output: F) {
        for input in &self.inputs {
            output(&input.name, input.as_uniform_value());
        }
    }
}

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));
static mut ISF_INFO: Lazy<ffgl_glium::ffi::ffgl1::PluginInfoStruct> = Lazy::new(|| {
    let mut name = [0; 16];
    let name_from_env = env!("ISF_NAME").as_bytes();

    name[0..name_from_env.len()].copy_from_slice(&name_from_env);

    let mut rng = Seeder::from(ISF_SOURCE).make_rng::<StdRng>();
    let mut code = [0; 4];
    rng.fill_bytes(&mut code);
    plugin_info(&code, &name)
});

static INSTANCE: Lazy<IsfState> = Lazy::new(|| IsfState::new());

#[derive(Debug, Clone)]
enum IsfInputValue {
    Event(bool),
    Bool(bool),
    Long(i32),
    Float(f32),
    Point2d([f32; 2]),
    Color([f32; 4]),
}

#[derive(Debug, Clone)]
struct IsfInputParam {
    ty: isf::InputType,
    name: String,
    params: Vec<BasicParam>,
}

impl AsUniformValue for IsfInputParam {
    fn as_uniform_value(&self) -> UniformValue<'_> {
        let ty = &self.ty;
        let params = self.params.as_slice();
        match (ty, params) {
            (
                isf::InputType::Long(_),
                [BasicParam {
                    param_value: Some(ParamValue::Float(x)),
                    ..
                }],
            ) => UniformValue::SignedInt(unsafe { transmute(*x) }),
            (
                isf::InputType::Float(_),
                [BasicParam {
                    param_value: Some(ParamValue::Float(x)),
                    ..
                }],
            ) => UniformValue::Float(*x),
            (
                isf::InputType::Point2d(_),
                [BasicParam {
                    param_value: Some(ParamValue::Float(x)),
                    ..
                }, BasicParam {
                    param_value: Some(ParamValue::Float(y)),
                    ..
                }],
            ) => UniformValue::Vec2([*x, *y]),
            (
                isf::InputType::Color(_),
                [BasicParam {
                    param_value: Some(ParamValue::Float(r)),
                    ..
                }, BasicParam {
                    param_value: Some(ParamValue::Float(g)),
                    ..
                }, BasicParam {
                    param_value: Some(ParamValue::Float(b)),
                    ..
                }, BasicParam {
                    param_value: Some(ParamValue::Float(a)),
                    ..
                }],
            ) => UniformValue::Vec4([*r, *g, *b, *a]),
            _ => panic!(
                "Invalid uniform value for ISF input {ty:?}\n params {params:?}",
                params = params.iter().map(|x| x.param_value).collect::<Vec<_>>()
            ),
        }
    }
}

impl IsfInputParam {
    fn new(Input { ty, name }: isf::Input) -> Self {
        let params = match &ty {
            isf::InputType::Event => vec![BasicParam {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Event,
                param_value: Some(ParamValue::Bool(false)),
                ..Default::default()
            }],
            isf::InputType::Bool(x) => vec![BasicParam {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Boolean,
                param_value: Some(ParamValue::Bool(false)),
                ..Default::default()
            }],
            isf::InputType::Long(x) => vec![BasicParam {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Integer,
                param_value: Some(ParamValue::Int(0)),
                ..Default::default()
            }],
            isf::InputType::Float(x) => vec![BasicParam {
                name: CString::new(name.clone()).unwrap(),
                param_type: ffgl_glium::parameters::ParameterTypes::Standard,
                param_value: Some(ParamValue::Float(0.0)),
                ..Default::default()
            }],
            isf::InputType::Point2d(x) => vec![
                BasicParam {
                    name: CString::new(format!("{name} x")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::X,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
                BasicParam {
                    name: CString::new(format!("{name} y")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Y,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
            ],
            isf::InputType::Color(x) => vec![
                BasicParam {
                    name: CString::new(format!("{name} r")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Red,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
                BasicParam {
                    name: CString::new(format!("{name} g")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Green,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
                BasicParam {
                    name: CString::new(format!("{name} b")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Blue,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
                BasicParam {
                    name: CString::new(format!("{name} a")).unwrap(),
                    param_type: ffgl_glium::parameters::ParameterTypes::Alpha,
                    param_value: Some(ParamValue::Float(0.0)),
                    group: Some(name.clone()),
                    ..Default::default()
                },
            ],
            _ => vec![],
        };

        Self { ty, params, name }
    }
}

impl IsfState {
    fn new() -> Self {
        let info = isf::parse(&ISF_SOURCE).unwrap();
        let params = info
            .inputs
            .iter()
            .cloned()
            .map(|input| IsfInputParam::new(input))
            .collect();

        Self {
            info,
            inputs: params,
        }
    }

    fn param(&self, mut index: usize) -> &BasicParam {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &self.inputs[input_index];
        let param = &input.params[index];

        param
    }

    fn param_mut(&mut self, mut index: usize) -> &mut BasicParam {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &mut self.inputs[input_index];
        let param = &mut input.params[index];

        param
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
    type Param = BasicParam;

    fn num_params() -> usize {
        INSTANCE.inputs.iter().map(|x| x.params.len()).sum()
    }

    fn param(index: usize) -> &'static Self::Param {
        let param = INSTANCE.param(index);
        // logln!("GOT PARAM {index} {param:?}");
        param
    }

    fn set_param(&mut self, index: usize, value: ffgl_glium::parameters::ParamValue) {
        // logln!("SET PARAM {index}: {value:?}");
        self.state.param_mut(index).set(value);
    }
}

impl FFGLGliumHandler for IsfFFGLInstance {
    fn info() -> &'static mut ffgl_glium::ffi::ffgl1::PluginInfoStruct {
        unsafe { &mut ISF_INFO }
    }

    fn new(inst_data: &ffgl_glium::FFGLData, ctx: std::rc::Rc<glium::backend::Context>) -> Self {
        ffgl_glium::logln!("CREATED SHADER");

        let shader = shader::IsfShader::new(&ctx, &INSTANCE.info, ISF_SOURCE).unwrap();

        Self {
            shader,
            state: INSTANCE.clone(),
        }
    }

    fn render_frame(&mut self, inst_data: &ffgl_glium::FFGLData, target: &mut impl glium::Surface) {
        self.shader
            .draw(target, &self.state)
            .expect("Error drawing ISF frame")
    }
}

ffgl_handler!(FFGLGlium<IsfFFGLInstance>);
