use std::cell::OnceCell;
use std::ffi::CString;

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
    pub inputs: Vec<param::IsfFFGLParam>,
    pub plugin_info: PluginInfo,
}

impl Uniforms for IsfFFGLState {
    fn visit_values<'a, F: FnMut(&str, UniformValue<'a>)>(&'a self, mut output: F) {
        for input in &self.inputs {
            if let param::IsfFFGLParam::Isf(param) = input {
                let uniform = param.as_uniform_optional();
                if let Some(uniform) = uniform {
                    output(&param.name, uniform);
                }
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
        let shader_params: Vec<param::IsfShaderParam> = info
            .inputs
            .iter()
            .cloned()
            .map(|input| param::IsfShaderParam::new(input))
            .collect();

        let plugin_type = if shader_params.iter().any(|x| x.ty == isf::InputType::Image) {
            PluginType::Effect
        } else {
            PluginType::Source
        };

        let basic_params = vec![param::IsfFFGLParam::Overlay(
            param::OverlayParams::Scale,
            BasicParamInfo {
                name: CString::new("Resize").unwrap(),
                default: Some(1.0),
                group: Some("opts".to_string()),
                ..Default::default()
            },
            1.0,
        )];

        let params: Vec<param::IsfFFGLParam> = basic_params
            .into_iter()
            .chain(
                shader_params
                    .into_iter()
                    .map(|p| param::IsfFFGLParam::Isf(p)),
            )
            .collect();

        let mut name = [0; 16];
        let name_str = format!("*{}", env!("ISF_NAME"));
        let name_b = name_str.as_bytes();

        let name_len = name_b.len().min(name.len());

        name[0..name_len].copy_from_slice(&name_b[0..name_len]);

        let mut rng = Seeder::from(ISF_SOURCE).make_rng::<StdRng>();
        let mut code = [0; 4];
        rng.fill_bytes(&mut code);

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

    fn param_info(&self, mut index: usize) -> &Self::Param {
        let mut input_index = 0;
        while self.inputs[input_index].num_params() <= index {
            index -= self.inputs[input_index].num_params();
            input_index += 1;
        }

        let input = &self.inputs[input_index];
        let param = &input.param_info(index);

        param
    }

    fn num_params(&'static self) -> usize {
        self.inputs.iter().map(|x| x.num_params()).sum()
    }

    fn plugin_info(&'static self) -> PluginInfo {
        self.plugin_info.clone()
    }

    fn new_instance(&'static self, inst_data: &ffgl_glium::FFGLData) -> Self::Instance {
        instance::IsfFFGLInstance::new(self, inst_data)
    }
}
