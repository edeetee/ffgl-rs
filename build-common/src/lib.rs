use glsl::transpiler::glsl::show_translation_unit;

pub mod glsl_120;
pub mod isf_glsl_preprocess;

pub fn translation_unit_to_string(tu: &glsl::syntax::TranslationUnit) -> String {
    let mut s = String::new();
    show_translation_unit(&mut s, tu);
    s
}
