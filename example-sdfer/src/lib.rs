use std::cmp::max;

use ffgl_core::{
    handler::simplified::*,
    info::PluginInfo,
    parameters::{handler::ParamInfoHandler, ParamInfo},
    plugin_main,
};
use ffgl_glium::FFGLGlium;
use glium::{
    framebuffer::{RenderBuffer, SimpleFrameBuffer},
    texture::{RawImage2d, Texture2dDataSink},
    Surface, Texture2d,
};
use sdfer::{esdt::Params, Unorm8};

//https://github.com/LykenSol/sdfer/tree/main/src
struct SdferInstance {
    // input_file: std::path::PathBuf,
    buffers: Option<sdfer::esdt::ReusableBuffers>,
    glium: ffgl_glium::FFGLGlium,
    inputs: Vec<f32>,
}

enum SdferParams {
    Res,
    Radius,
}

impl ParamInfo for SdferParams {
    fn name(&self) -> &std::ffi::CStr {
        std::ffi::CStr::from_bytes_with_nul(match self {
            SdferParams::Radius => b"Radius\0",
            SdferParams::Res => b"res\0",
        })
        .expect("Invalid CStr")
    }

    fn default_val(&self) -> f32 {
        match self {
            SdferParams::Radius => 0.1,
            SdferParams::Res => 0.5,
        }
    }
}

static PARAMS: [SdferParams; 2] = [SdferParams::Res, SdferParams::Radius];

fn process(
    texture: &Texture2d,
    buffers: Option<sdfer::esdt::ReusableBuffers>,
    opts: Params,
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

    sdfer::esdt::glyph_to_sdf(&mut img, opts, buffers)
}

impl SimpleFFGLInstance for SdferInstance {
    fn new(inst_data: &ffgl_core::FFGLData) -> Self {
        Self {
            buffers: Default::default(),
            glium: FFGLGlium::new(inst_data),
            inputs: PARAMS.iter().map(|p| (p.default_val())).collect(),
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
        let mut scale = self.inputs[PARAMS
            .iter()
            .position(|p| matches!(p, SdferParams::Res))
            .expect("No scale")];

        let radius = self.inputs[PARAMS
            .iter()
            .position(|p| matches!(p, SdferParams::Radius))
            .expect("No radius")];

        scale = scale.powi(2);

        let dest_res = inst_data.get_dimensions();
        let render_res = (
            max((dest_res.0 as f32 * scale) as u32, 1),
            max((dest_res.1 as f32 * scale) as u32, 1),
        );

        // let scaled_target = if scale != 1.0 {
        //     Some(SimpleFrameBuffer)
        // } else {
        //     None
        // };

        self.glium
            .draw(render_res, dest_res, frame_data, &mut |fb, textures| {
                let render_texture = Texture2d::empty(&self.glium.ctx, render_res.0, render_res.1)
                    .expect("Texture could not be created");

                //resize to render size
                textures.first().expect("No texture in").as_surface().fill(
                    &render_texture.as_surface(),
                    glium::uniforms::MagnifySamplerFilter::Linear,
                );

                let esdt_opts = Params {
                    solidify: false,
                    radius: render_res.0 as f32 * radius,
                    ..Default::default()
                };

                let (sdf, buffers) = process(&render_texture, self.buffers.take(), esdt_opts);

                self.buffers = Some(buffers);

                let storage: Vec<_> = sdf
                    .storage()
                    .iter()
                    .map(|x| x.to_bits())
                    .map(|x| (x, x, x, x))
                    .collect();

                let data_src = RawImage2d::from_raw(
                    std::borrow::Cow::Owned(storage),
                    sdf.width() as u32,
                    sdf.height() as u32,
                );

                let sdf_texture = Texture2d::new(&self.glium.ctx, data_src)
                    .expect("Texture could not be created");

                sdf_texture
                    .as_surface()
                    .fill(fb, glium::uniforms::MagnifySamplerFilter::Linear);

                // Texture2d::empty_with_format(facade, format, mipmaps, width, height);

                Ok(())
            })
    }

    fn num_params() -> usize {
        PARAMS.len()
    }

    fn param_info(index: usize) -> &'static dyn ParamInfo {
        PARAMS.param_info(index)
    }

    fn get_param(&self, index: usize) -> f32 {
        self.inputs[index]
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.inputs[index] = value;
    }
}

plugin_main!(SimpleFFGLHandler<SdferInstance>);
