use std::error::Error;

use glsl::parser::Parse;
use glsl::syntax::{Expr, ShaderStage};
use isf;

const STANDARD_PREFIX: &'static str = include_str!("prefix.glsl");

use isf::Isf;

pub fn validate_isf_source(original_source: &str) -> Result<(), Box<dyn Error>> {
    let isf = isf::parse(original_source)?;

    let source = convert_source_to_glsl(&isf, original_source);

    ShaderStage::parse(source)?;

    Ok(())
}

pub fn convert_source_to_glsl(def: &Isf, source: &str) -> String {
    let prefix = generate_isf_prefix(def);

    (format!("{prefix}\n{source}"))
        .replace("gl_FragColor", "isf_FragColor")
        .replace("varying", "out")
}

pub fn generate_isf_prefix(def: &Isf) -> String {
    let mut prefix = String::new();

    prefix.push_str(STANDARD_PREFIX);

    let inputs = def.inputs.iter().map(|input| {
        let gl_ty = match input.ty {
            isf::InputType::Image => "sampler2D",
            isf::InputType::Float(_) => "float",
            isf::InputType::Point2d(_) => "vec2",
            isf::InputType::Color(_) => "vec4",
            isf::InputType::Audio(_) => "sampler2D",
            isf::InputType::AudioFft(_) => "sampler2D",
            isf::InputType::Event => "bool",
            isf::InputType::Bool(_) => "bool",
            isf::InputType::Long(_) => "int",
        };
        let name = &input.name;
        (name, gl_ty)
    });

    let passes = def
        .passes
        .iter()
        .filter_map(|pass| pass.target.as_ref().map(|name| (name, "sampler2D")));

    for (name, gl_ty) in inputs.chain(passes) {
        prefix.push_str(&format!("uniform {gl_ty} {name};\n"));
    }

    prefix.push('\n');

    prefix
}
