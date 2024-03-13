use std::cmp::max;

use ffgl_core::{
    handler::simplified::*,
    info::{plugin_info, PluginInfo},
    parameters::{self, builtin::OverlayParams, handler::ParamInfoHandler, ParamInfo},
    plugin_main,
};
use ffgl_glium::FFGLGlium;
use glium::{buffer, Texture2d};
use sdfer::{esdt::Params, Unorm8};

//https://github.com/LykenSol/sdfer/tree/main/src
struct SdferInstance {
    // input_file: std::path::PathBuf,
    sdf: sdfer::Image2d<sdfer::Unorm8>,
    buffers: Option<sdfer::esdt::ReusableBuffers>,
    glium: ffgl_glium::FFGLGlium,
    inputs: Vec<f32>,
}

static params: [OverlayParams; 1] = [parameters::builtin::OverlayParams::Scale; 1];

fn process(
    texture: &Texture2d,
    buffers: Option<sdfer::esdt::ReusableBuffers>,
) -> (sdfer::Image2d<sdfer::Unorm8>, sdfer::esdt::ReusableBuffers) {
    let pixel_buf: Vec<_> = texture.read();

    let flat_buf: Vec<_> = pixel_buf
        .iter()
        .flatten()
        .map(|(r, ..)| Unorm8::from_bits(*r))
        .collect();

    let mut img = sdfer::Image2d::from_storage(
        texture.width() as usize,
        texture.height() as usize,
        flat_buf,
    );

    sdfer::esdt::glyph_to_sdf(&mut img, Params::default(), buffers)
}

impl SimpleFFGLInstance for SdferInstance {
    fn new(inst_data: &ffgl_core::FFGLData) -> Self {
        Self {
            sdf: Default::default(),
            buffers: Default::default(),
            glium: FFGLGlium::new(inst_data),
            inputs: params.iter().map(|p| (p.default_val())).collect(),
        }
    }

    fn plugin_info() -> ffgl_core::info::PluginInfo {
        PluginInfo {
            unique_id: *b"SDFR",
            name: *b"Sdfer           ",
            ty: ffgl_core::info::PluginType::Effect,
            ..Default::default()
        }
    }

    fn draw(&mut self, inst_data: &ffgl_core::FFGLData, frame_data: ffgl_core::GLInput) {
        let scale = self.inputs[params
            .iter()
            .position(|p| matches!(p, OverlayParams::Scale))
            .expect("No scale")];

        let dest_res = inst_data.get_dimensions();
        let render_res = (
            max((dest_res.0 as f32 * scale) as u32, 1),
            max((dest_res.1 as f32 * scale) as u32, 1),
        );

        self.glium
            .draw(dest_res, render_res, frame_data, &mut |fb, textures| {
                let (sdf, buffers) =
                    process(textures.first().expect("No texture"), self.buffers.take());

                self.sdf = sdf;
                self.buffers = Some(buffers);

                let gray_img: image::GrayImage = sdf.into();

                // let sdf_texture = Texture2d::new(
                //     &self.glium.ctx,
                //     gray_img.enumerate_rows()
                // );

                Texture2d::empty_with_format(facade, format, mipmaps, width, height)

                Ok(())
            })
    }

    fn num_params() -> usize {
        params.len()
    }

    fn param_info(index: usize) -> &'static dyn ParamInfo {
        params.param_info(index)
    }

    fn get_param(&self, index: usize) -> f32 {
        self.inputs[index]
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.inputs[index] = value;
    }
}

plugin_main!(SimpleFFGLHandler<SdferInstance>);
