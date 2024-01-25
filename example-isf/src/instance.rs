use crate::handler::IsfFFGLState;
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
    pub(crate) fn new(state: &IsfFFGLState, inst_data: &ffgl_glium::FFGLData) -> Self {
        tracing::debug!("CREATED INSTANCE");

        let glium = FFGLGliumInstance::new(inst_data);

        let shader = shader::IsfShader::new(&glium.ctx, &state.info, &state.source).unwrap();

        Self {
            shader,
            state: state.clone(),
            glium,
        }
    }
}
