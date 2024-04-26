use std::error::Error;

use glsl::parser::Parse;
use glsl::syntax::{Expr, FunIdentifier, Identifier, ShaderStage};
use glsl::visitor::{Host, HostMut, VisitorMut};
use isf;

const STANDARD_PREFIX: &'static str = include_str!("isf_prefix.glsl");
const PREFIX_120: &'static str = include_str!("isf_prefix_120.glsl");
const PREFIX_140: &'static str = include_str!("isf_prefix_140.glsl");

use isf::Isf;

use crate::{glsl_120, translation_unit_to_string, GlslVersion};

struct UniformTextureSizeMutator;

impl VisitorMut for UniformTextureSizeMutator {
    fn visit_expr(&mut self, e: &mut glsl::syntax::Expr) -> glsl::visitor::Visit {
        match e {
            Expr::FunCall(FunIdentifier::Identifier(Identifier(ident)), exprs) => {
                if ident == "IMG_PIXEL" {
                    // panic!("IMG_PIXEL");
                    *ident = "texture".to_string();

                    let sampler_name = match exprs.first().expect("No name in IMG_PIXEL") {
                        Expr::Variable(v) => v.0.clone(),
                        _ => unreachable!("First argument to IMG_PIXEL is not a variable"),
                    };

                    let last = exprs.last_mut().expect("No last expr");

                    match last {
                        Expr::Variable(Identifier(coord_ident)) => {
                            *last =
                                Expr::parse(format!("{coord_ident}/{sampler_name}_size")).unwrap();
                        }
                        _ => panic!("Last expr is not a variable"),
                    }

                    // panic!("EXPR: {:?}", e);
                }
            }
            _ => {}
        }

        glsl::visitor::Visit::Children
    }
}

pub fn validate_isf_source(original_source: &str) -> Result<(), Box<dyn Error>> {
    let isf = isf::parse(original_source)?;

    let source = compile_isf_fragment(&isf, original_source, GlslVersion::Glsl140);

    ShaderStage::parse(source)?;

    Ok(())
}

pub fn compile_isf_fragment(def: &Isf, source: &str, glsl_version: GlslVersion) -> String {
    let prefix = generate_isf_prefix(def);

    let ver_prefix = match glsl_version {
        GlslVersion::Glsl120 => PREFIX_120,
        _ => PREFIX_140,
    };

    let source = format!("{ver_prefix}\n{prefix}\n{source}");

    let mut shader = ShaderStage::parse(source).expect("Failed to parse source");

    shader.visit_mut(&mut UniformTextureSizeMutator);

    glsl_version.visit_mut(&mut shader);

    translation_unit_to_string(&shader)
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

        if gl_ty == "sampler2D" {
            prefix.push_str(&format!("uniform vec2 {name}_size;\n"));
        }
    }

    prefix.push('\n');

    prefix
}
