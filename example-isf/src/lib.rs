use std::fmt::{Debug, Formatter};
mod fullscreen_shader;
mod meta;
mod shader;
mod texture;
mod util;
use once_cell::sync::Lazy;

use ffgl_glium::{
    ffgl_handler, logln, parameters::BasicParam, plugin_info, FFGLGlium, FFGLGliumHandler,
    ParamHandler,
};
use glium::uniforms::EmptyUniforms;
use isf::Isf;
use rand::Rng;

pub struct StaticIsf {
    pub info: Isf,
    pub params: Vec<BasicParam>,
}

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));
static mut ISF_INFO: Lazy<ffgl_glium::ffi::ffgl1::PluginInfoStruct> = Lazy::new(|| {
    let mut name = [0; 16];
    let name_from_env = env!("ISF_NAME").as_bytes();

    name[0..name_from_env.len()].copy_from_slice(&name_from_env);

    let mut rng = rand::thread_rng();
    let code = [rng.gen(), rng.gen(), rng.gen(), rng.gen()];
    plugin_info(&code, &name)
});

static INSTANCE: Lazy<StaticIsf> = Lazy::new(|| StaticIsf::new());

impl StaticIsf {
    fn new() -> Self {
        let info = isf::parse(&ISF_SOURCE).unwrap();
        let params = info
            .inputs
            .iter()
            .map(|input| BasicParam::from_name(&input.name))
            .collect();

        Self { info, params }
    }
}

pub struct IsfFFGLInstance {
    pub shader: shader::IsfShader,
}

impl Debug for IsfFFGLInstance {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("IsfFFGLInstance").finish()
    }
}

impl ParamHandler for IsfFFGLInstance {
    type Param = BasicParam;

    fn params() -> &'static [Self::Param] {
        &INSTANCE.params
    }

    fn set_param(&mut self, index: usize, value: ffgl_glium::parameters::ParamValue) {
        logln!("SET PARAM {index}: {value:?}", index = index, value = value);
    }
}

impl FFGLGliumHandler for IsfFFGLInstance {
    fn info() -> &'static mut ffgl_glium::ffi::ffgl1::PluginInfoStruct {
        unsafe { &mut ISF_INFO }
    }

    fn new(inst_data: &ffgl_glium::FFGLData, ctx: std::rc::Rc<glium::backend::Context>) -> Self {
        ffgl_glium::logln!("CREATED SHADER");

        let shader = shader::IsfShader::new(&ctx, &INSTANCE.info, ISF_SOURCE).unwrap();

        Self { shader }
    }

    fn render_frame(&mut self, inst_data: &ffgl_glium::FFGLData, target: &mut impl glium::Surface) {
        self.shader
            .draw(target, &EmptyUniforms)
            .expect("Error drawing ISF frame")
    }
}

ffgl_handler!(FFGLGlium<IsfFFGLInstance>);
