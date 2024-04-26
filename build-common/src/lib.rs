use glsl::{
    parser::Parse,
    syntax::{Identifier, NonEmpty, TranslationUnit, TypeQualifier},
    transpiler::glsl::show_translation_unit,
    visitor::{HostMut, Visitor, VisitorMut},
};
use glsl_120::Glsl120Mutator;
use glsl_140plus::Glsl140PlusMutator;

pub mod glsl_120;
pub mod glsl_140plus;
pub mod glsl_output;
pub mod isf_glsl_preprocess;

#[derive(Debug, Clone, Copy)]
pub enum GlslVersion {
    Glsl120,
    // Glsl140,
    Glsl140,
}

struct GlslVersionMutator {
    version: GlslVersion,
}

impl VisitorMut for GlslVersionMutator {
    fn visit_preprocessor_version(
        &mut self,
        v: &mut glsl::syntax::PreprocessorVersion,
    ) -> glsl::visitor::Visit {
        *v = glsl::syntax::PreprocessorVersion {
            version: match self.version {
                GlslVersion::Glsl120 => 120,
                GlslVersion::Glsl140 => 140,
            },
            ..v.clone()
        };

        glsl::visitor::Visit::Children
    }
}

impl GlslVersion {
    pub fn visit_mut(&self, unit: &mut TranslationUnit) {
        match self {
            GlslVersion::Glsl120 => {
                unit.visit_mut(&mut Glsl120Mutator);
            }
            GlslVersion::Glsl140 => unit.visit_mut(&mut Glsl140PlusMutator),
        }
        unit.visit_mut(&mut GlslVersionMutator { version: *self })
    }
}

pub fn translation_unit_to_string(tu: &glsl::syntax::TranslationUnit) -> String {
    let mut s = String::new();
    show_translation_unit(&mut s, tu);
    s
}

pub fn transform_glsl(src: &str, version: GlslVersion) -> String {
    let mut parsed = glsl::syntax::ShaderStage::parse(src).expect("Failed to parse source");

    version.visit_mut(&mut parsed);

    translation_unit_to_string(&parsed)
}
