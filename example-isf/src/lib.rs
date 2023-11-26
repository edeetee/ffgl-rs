use std::fmt::{Debug, Formatter};
mod fullscreen_shader;
mod meta;
mod shader;
mod texture;
mod util;
use once_cell::sync::Lazy;

pub struct StaticIsf {
    pub info: Isf,
}

const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));
static mut ISF_INFO: Lazy<ffgl_glium::ffi::ffgl1::PluginInfoStruct> = Lazy::new(|| {
    let mut name = [0; 16];
    let name_from_env = env!("ISF_NAME").as_bytes();
    name[0..name_from_env.len()].copy_from_slice(&name_from_env);
    plugin_info(b"0000", &name)
});

static INSTANCE: Lazy<StaticIsf> = Lazy::new(|| StaticIsf::new());

impl StaticIsf {
    fn new() -> Self {
        let info = isf::parse(&ISF_SOURCE).unwrap();

        Self { info }
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

use ffgl_glium::{ffgl_handler, parameters::BasicParam, plugin_info, FFGLGlium, FFGLGliumHandler};
use glium::uniforms::EmptyUniforms;
use isf::Isf;

impl FFGLGliumHandler for IsfFFGLInstance {
    type Param = BasicParam;

    fn info() -> &'static mut ffgl_glium::ffi::ffgl1::PluginInfoStruct {
        unsafe { &mut ISF_INFO }
    }

    fn params() -> &'static [Self::Param] {
        &[]
    }

    fn param_mut(&mut self, index: usize) -> &mut Self::Param {
        unimplemented!("No params")
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
