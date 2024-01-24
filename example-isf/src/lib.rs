use std::fmt::{Debug, Formatter};
mod fullscreen_shader;
mod shader;
mod util;

use ffgl_glium::{
    ffgl_handler,
    log::init_default_subscriber,
    parameters::BasicParamInfo,
    traits::{FFGLHandler, FFGLInstance},
    FFGLGliumInstance, PluginInfo, PluginType,
};
use glium::uniforms::{UniformValue, Uniforms};
use isf::Isf;
use param::AsUniformOptional;
use rand::{rngs::StdRng, RngCore};
use rand_seeder::Seeder;

use util::MultiUniforms;

mod param;

#[derive(Debug, Clone)]
pub struct IsfState {
    pub info: Isf,
    pub inputs: Vec<param::IsfInputParam>,
    pub plugin_info: PluginInfo,
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

impl FFGLHandler for IsfState {
    type Instance = IsfFFGLInstance;

    type Param = BasicParamInfo;

    fn init() -> Self {
        init_default_subscriber();

        let info = isf::parse(ISF_SOURCE).unwrap();
        let params: Vec<param::IsfInputParam> = info
            .inputs
            .iter()
            .cloned()
            .map(|input| param::IsfInputParam::new(input))
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

        let plugin_info = PluginInfo {
            unique_id: code,
            name: name,
            ty: plugin_type,
            about: info.categories.join(", "),
            description: info.description.clone().unwrap_or_default(),
        };

        tracing::debug!("ISF INFO: {info:#?}");
        tracing::debug!("ISF PARAMS: {params:#?}");

        Self {
            info,
            inputs: params,
            plugin_info,
        }
    }

    fn param_info(&'static self, mut index: usize) -> &'static Self::Param {
        let mut input_index = 0;
        while self.inputs[input_index].params.len() <= index {
            index -= self.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &self.inputs[input_index];
        let param = &input.params[index];

        param
    }

    fn plugin_info(&'static self) -> PluginInfo {
        self.plugin_info.clone()
    }

    fn new_instance(&'static self, inst_data: &ffgl_glium::FFGLData) -> Self::Instance {
        IsfFFGLInstance::new(self, inst_data)
    }

    fn num_params(&'static self) -> usize {
        self.inputs.iter().map(|x| x.params.len()).sum()
    }
}

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));

pub struct IsfFFGLInstance {
    pub shader: shader::IsfShader,
    pub state: IsfState,
    pub glium: FFGLGliumInstance,
}

impl Debug for IsfFFGLInstance {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("IsfFFGLInstance").finish()
    }
}

impl FFGLInstance for IsfFFGLInstance {
    fn get_param(&self, mut index: usize) -> f32 {
        let mut input_index = 0;
        while self.state.inputs[input_index].params.len() <= index {
            index -= self.state.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &self.state.inputs[input_index];

        input.value.get(index)
    }

    fn set_param(&mut self, mut index: usize, value: f32) {
        let mut input_index = 0;
        while self.state.inputs[input_index].params.len() <= index {
            index -= self.state.inputs[input_index].params.len();
            input_index += 1;
        }

        let input = &mut self.state.inputs[input_index];

        input.value.set(index, value);
    }

    fn draw(&mut self, inst_data: &ffgl_glium::FFGLData, frame_data: ffgl_glium::GLInput) {
        self.glium
            .draw(inst_data, frame_data, &mut |target, textures| {
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
            });
    }
}

impl IsfFFGLInstance {
    fn new(state: &IsfState, inst_data: &ffgl_glium::FFGLData) -> Self {
        tracing::debug!("CREATED INSTANCE");

        let glium = FFGLGliumInstance::new(inst_data);

        let shader = shader::IsfShader::new(&glium.ctx, &state.info, ISF_SOURCE).unwrap();

        Self {
            shader,
            state: state.clone(),
            glium,
        }
    }
}

ffgl_handler!(IsfState);
