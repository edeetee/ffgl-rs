use ffgl_core;

use ffgl_core::info;

use ffgl_core::parameters;
use ffgl_core::parameters::handler::ParamInfoHandler;
use ffgl_core::parameters::ParamInfo;

use tracing::span;

use crate::instance;
use crate::param;
use crate::param::AsUniformOptional;
use crate::shader::IsfShaderLoadError;

use ffgl_core::log::init_default_subscriber;

use ffgl_core::handler::FFGLHandler;

use glium::uniforms::UniformValue;

use glium::uniforms::Uniforms;

use isf::Isf;

pub const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));
pub const ISF_NAME: &'static str = env!("ISF_NAME");

#[derive(Debug, Clone)]
pub struct IsfFFGLState {
    pub source: String,
    pub info: Isf,
    pub inputs: Vec<param::IsfFFGLParam>,
    pub plugin_info: info::PluginInfo,
    pub span: tracing::Span,
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
    type NewInstanceError = IsfShaderLoadError;

    fn init() -> Self {
        let span = span!(tracing::Level::TRACE, ISF_NAME);
        let _ = span.enter();

        init_default_subscriber();

        let info = isf::parse(ISF_SOURCE).unwrap();
        let shader_params: Vec<param::IsfShaderParam> = info
            .inputs
            .iter()
            .cloned()
            .map(|input| param::IsfShaderParam::new(input))
            .collect();

        let plugin_type = if shader_params.iter().any(|x| x.ty == isf::InputType::Image) {
            info::PluginType::Effect
        } else {
            info::PluginType::Source
        };

        let basic_params = vec![param::IsfFFGLParam::Overlay(
            parameters::builtin::OverlayParams::Scale,
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
        let name_str = format!("*{}", ISF_NAME);
        let name_b = name_str.as_bytes();

        let name_len = name_b.len().min(name.len());

        name[0..name_len].copy_from_slice(&name_b[0..name_len]);

        // let mut rng = Seeder::from(ISF_SOURCE).make_rng::<StdRng>();
        let mut code = [0; 4];
        code[1..].copy_from_slice(&name[1..4]);
        code[0] = b'*';

        // rng.fill_bytes(&mut code);

        let plugin_info = info::PluginInfo {
            unique_id: code,
            name: name,
            ty: plugin_type,
            about: info.categories.join(", "),
            description: info.description.clone().unwrap_or_default(),
        };

        tracing::trace!("ISF INFO: {info:#?}");
        tracing::trace!("ISF PARAMS: {params:#?}");

        Self {
            source: ISF_SOURCE.to_string(),
            info,
            inputs: params,
            plugin_info,
            span,
        }
    }

    fn param_info(&self, index: usize) -> &dyn ParamInfo {
        let _ = self.span.enter();
        self.inputs.param_info(index)
    }

    fn num_params(&'static self) -> usize {
        let _ = self.span.enter();
        self.inputs.num_params()
    }

    fn plugin_info(&'static self) -> info::PluginInfo {
        let _ = self.span.enter();
        self.plugin_info.clone()
    }

    fn new_instance(
        &'static self,
        inst_data: &ffgl_core::FFGLData,
    ) -> Result<Self::Instance, Self::NewInstanceError> {
        let _ = self.span.enter();
        instance::IsfFFGLInstance::new(self, inst_data)
    }
}
