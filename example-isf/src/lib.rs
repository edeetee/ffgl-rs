use std::{
    fmt::{Debug, Formatter},
    sync::OnceLock,
};

mod fullscreen_shader;
mod meta;
mod shader;
mod texture;
mod util;

pub struct StaticIsf {
    pub info: Isf,
}

const ISF_SOURCE: &'static str = include_str!("/Library/Graphics/ISF/Quad Tile.fs");
static mut ISF_INFO: ffgl_glium::ffi::ffgl1::PluginInfoStruct =
    plugin_info(b"0000", b"Quad Tile       ");

static mut INSTANCE: OnceLock<StaticIsf> = OnceLock::new();

impl StaticIsf {
    fn new() -> Self {
        let info = isf::parse(&ISF_SOURCE).unwrap();

        Self { info }
    }

    pub fn get() -> &'static Self {
        unsafe { INSTANCE.get_or_init(Self::new) }
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

        let shader = shader::IsfShader::new(&ctx, &StaticIsf::get().info, ISF_SOURCE).unwrap();

        Self { shader }
    }

    fn render_frame(&mut self, inst_data: &ffgl_glium::FFGLData, target: &mut impl glium::Surface) {
        self.shader
            .draw(target, &EmptyUniforms)
            .expect("Error drawing ISF frame")
    }
}

ffgl_handler!(FFGLGlium<IsfFFGLInstance>);
