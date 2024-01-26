use crate::handler::IsfFFGLState;
use crate::param;
use crate::param::IsfInputValue;
use crate::param::OverlayParams;
use crate::shader;
use crate::util::MultiUniforms;

use glium::uniforms::UniformValue;

use ffgl_glium;

use ffgl_glium::traits::FFGLInstance;

use std::fmt::Formatter;

use std::fmt::Debug;

use ffgl_glium::FFGLGliumInstance;

pub struct IsfFFGLInstance {
    pub shader: shader::IsfShader,
    pub state: IsfFFGLState,
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
        while self.state.inputs[input_index].num_params() <= index {
            index -= self.state.inputs[input_index].num_params();
            input_index += 1;
        }

        let input = &self.state.inputs[input_index];

        input.get(index)
    }

    fn set_param(&mut self, mut index: usize, value: f32) {
        let mut input_index = 0;
        while self.state.inputs[input_index].num_params() <= index {
            index -= self.state.inputs[input_index].num_params();
            input_index += 1;
        }

        let input = &mut self.state.inputs[input_index];

        input.set(index, value);
    }

    fn draw(&mut self, inst_data: &ffgl_glium::FFGLData, frame_data: ffgl_glium::GLInput) {
        let scale = match &self.state.inputs[0] {
            crate::param::IsfFFGLParam::Overlay(OverlayParams::Scale, _, val) => *val,
            _ => 1.0,
        };

        let new_res = inst_data.get_dimensions();
        let new_res = (
            (new_res.0 as f32 * scale) as u32,
            (new_res.1 as f32 * scale) as u32,
        );

        self.glium
            .draw(new_res, frame_data, &mut |target, textures| {
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

                self.shader.draw(&self.glium.ctx, target, &uniforms)?;

                Ok(())
            });
    }
}

impl IsfFFGLInstance {
    pub(crate) fn new(state: &IsfFFGLState, inst_data: &ffgl_glium::FFGLData) -> Self {
        tracing::debug!("CREATED INSTANCE");

        let glium = FFGLGliumInstance::new(inst_data);

        let shader = shader::IsfShader::new(
            &glium.ctx,
            &state.info,
            inst_data.get_dimensions(),
            &state.source,
        )
        .unwrap();

        Self {
            shader,
            state: state.clone(),
            glium,
        }
    }
}
