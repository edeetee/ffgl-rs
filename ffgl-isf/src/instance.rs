use crate::handler::IsfFFGLState;
use crate::param;
use crate::shader;
use crate::shader::IsfShaderLoadError;
use crate::util::MultiUniforms;

use ffgl_core::parameters::builtin::OverlayParams;
use ffgl_core::parameters::handler::ParamValueHandler;
use glium::uniforms::UniformValue;

use ffgl_core;

use ffgl_core::handler::FFGLInstance;
use glium::Surface;

use std::cmp::max;
use std::fmt::Formatter;

use std::fmt::Debug;

use ffgl_glium::FFGLGlium;

pub struct IsfFFGLInstance {
    pub shader: shader::IsfShader,
    pub state: IsfFFGLState,
    pub glium: FFGLGlium,
}

impl Debug for IsfFFGLInstance {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("IsfFFGLInstance").finish()
    }
}

impl FFGLInstance for IsfFFGLInstance {
    fn get_param(&self, index: usize) -> f32 {
        let _span = self.state.span.enter();
        self.state.inputs.get_param(index)
    }

    fn set_param(&mut self, index: usize, value: f32) {
        let _span = self.state.span.enter();
        self.state.inputs.set_param(index, value)
    }

    fn draw(&mut self, inst_data: &ffgl_core::FFGLData, frame_data: ffgl_core::GLInput) {
        let _span = self.state.span.enter();
        let scale = match &self.state.inputs[0] {
            crate::param::IsfFFGLParam::Overlay(OverlayParams::Scale, val) => (*val).powf(2.0),
            _ => 1.0,
        };

        let dest_res = frame_data
            .textures
            .first()
            .map(|t| (t.HardwareWidth, t.HardwareHeight))
            .unwrap_or(inst_data.get_dimensions());

        let render_res = (
            max((dest_res.0 as f32 * scale) as u32, 1),
            max((dest_res.1 as f32 * scale) as u32, 1),
        );

        self.glium
            .draw(render_res, dest_res, frame_data, &mut |target, textures| {
                let image_uniforms = self
                    .state
                    .inputs
                    .iter()
                    .filter_map(|i| match i {
                        param::IsfFFGLParam::Isf(param::IsfShaderParam {
                            ty: isf::InputType::Image,
                            name,
                            ..
                        }) => Some((
                            name.as_str(),
                            UniformValue::Texture2d(textures.first()?, None),
                        )),
                        _ => None,
                    })
                    .collect();

                let uniforms = MultiUniforms {
                    uniforms: image_uniforms,
                    next: &self.state,
                };

                self.shader.try_update_size(&self.glium.ctx, render_res);

                self.shader.draw(target, &uniforms)?;

                Ok(())
            });
        drop(_span)
    }
}

impl IsfFFGLInstance {
    pub(crate) fn new(
        state: &IsfFFGLState,
        inst_data: &ffgl_core::FFGLData,
    ) -> Result<Self, IsfShaderLoadError> {
        tracing::debug!("CREATED INSTANCE");

        let glium = FFGLGlium::new(inst_data);

        let shader = shader::IsfShader::new(
            &glium.ctx,
            &state.info,
            inst_data.get_dimensions(),
            &state.source,
        )?;

        Ok(Self {
            shader,
            state: state.clone(),
            glium,
        })
    }
}
