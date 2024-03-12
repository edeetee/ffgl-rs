use core::panic;

use glsl::{
    parser::Parse,
    syntax::{
        Declaration, Expr, FunIdentifier, FunctionPrototype, Identifier, NonEmpty,
        StorageQualifier, TranslationUnit,
    },
    transpiler::glsl::show_translation_unit,
    visitor::{HostMut, VisitorMut},
};

use crate::translation_unit_to_string;

pub struct Glsl120Mutator {
    pub is_fragment: bool,
}

pub fn transform_to_glsl_120(src: &str, is_fragment: bool) -> String {
    let mut parsed = glsl::syntax::ShaderStage::parse(src).expect("Failed to parse source");

    parsed.visit_mut(&mut Glsl120Mutator { is_fragment });

    translation_unit_to_string(&parsed)
}

impl VisitorMut for Glsl120Mutator {
    fn visit_preprocessor_version(
        &mut self,
        v: &mut glsl::syntax::PreprocessorVersion,
    ) -> glsl::visitor::Visit {
        *v = glsl::syntax::PreprocessorVersion {
            version: 120,
            ..v.clone()
        };

        glsl::visitor::Visit::Children
    }

    fn visit_translation_unit(
        &mut self,
        t: &mut glsl::syntax::TranslationUnit,
    ) -> glsl::visitor::Visit {
        //remove precision declarations
        *t = TranslationUnit(
            NonEmpty::from_non_empty_iter(t.clone().into_iter().filter(|d| match d {
                glsl::syntax::ExternalDeclaration::Declaration(Declaration::Precision(..)) => false,
                _ => true,
            }))
            .expect("No declarations left after filtering"),
        );

        glsl::visitor::Visit::Children
    }

    fn visit_expr(&mut self, e: &mut glsl::syntax::Expr) -> glsl::visitor::Visit {
        match e {
            Expr::FunCall(FunIdentifier::Identifier(Identifier(f)), _) if f == "texture" => {
                *f = "texture2D".to_string();
            }
            _ => {}
        }

        glsl::visitor::Visit::Children
    }

    fn visit_storage_qualifier(
        &mut self,
        s: &mut glsl::syntax::StorageQualifier,
    ) -> glsl::visitor::Visit {
        match s {
            StorageQualifier::In => *s = StorageQualifier::Attribute,
            StorageQualifier::Out => *s = StorageQualifier::Varying,
            _ => {}
        }

        glsl::visitor::Visit::Children
    }
}

#[cfg(test)]
mod tests {
    use glsl::{parser::Parse, syntax::ShaderStage, visitor::HostMut};

    use crate::glsl_120::Glsl120Mutator;

    #[test]
    fn test_to_glsl_120() {
        let src = r#"
            #version 330
            in vec2 texCoord;

            out vec4 color;

            uniform sampler2D tex;

            void main() {
                vec4 outTest;
                color = texture(tex, texCoord);
            }
            "#;

        let expected = r#"
            #version 120
            attribute vec2 texCoord;

            varying vec4 color;

            uniform sampler2D tex;

            void main() {
                vec4 outTest;
                color = texture2D(tex, texCoord);
            }
            "#;

        let mut src_parsed = ShaderStage::parse(src);

        let _ = src_parsed
            .as_mut()
            .map(|a| a.visit_mut(&mut Glsl120Mutator { is_fragment: true }));

        let expected_parsed = ShaderStage::parse(expected);

        assert_eq!(src_parsed, expected_parsed);
    }
}
