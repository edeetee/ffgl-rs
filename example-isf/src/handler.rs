use ffgl_glium;

use ffgl_glium::PluginType;

use rand::rngs::StdRng;

use rand::RngCore;
use rand_seeder::Seeder;

use crate::instance;
use crate::param;
use crate::param::AsUniformOptional;

use ffgl_glium::log::init_default_subscriber;

use ffgl_glium::parameters::BasicParamInfo;

use ffgl_glium::traits::FFGLHandler;

use glium::uniforms::UniformValue;

use glium::uniforms::Uniforms;

use ffgl_glium::PluginInfo;

use isf::Isf;

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));

#[derive(Debug, Clone)]
pub struct IsfFFGLState {
    pub source: String,
    pub info: Isf,
    pub inputs: Vec<param::IsfInputParam>,
    pub plugin_info: PluginInfo,
}

impl Uniforms for IsfFFGLState {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut output: F) {
        for input in &self.inputs {
            let uniform = input.as_uniform_optional();
            if let Some(uniform) = uniform {
                output(&input.name, uniform);
            }
        }
    }
}

impl FFGLHandler for IsfFFGLState {
    type Instance = instance::IsfFFGLInstance;

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
            source: ISF_SOURCE.to_string(),
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
        instance::IsfFFGLInstance::new(self, inst_data)
    }

    fn num_params(&'static self) -> usize {
        self.inputs.iter().map(|x| x.params.len()).sum()
    }
}
