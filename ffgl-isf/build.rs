/// Create the full fragment shader code and validate it before compiling

const ISF_SOURCE_PATH: &'static str = env!("ISF_SOURCE");
const ISF_SOURCE: &'static str = include_str!(env!("ISF_SOURCE"));
// const ISF_NAME: &'static str = env!("ISF_NAME");

fn main() {
    if let Err(e) = build_common::isf_glsl_preprocess::validate_isf_source(ISF_SOURCE) {
        // eprintln!("Error validating ISF source: {}", e);
        println!("cargo:warning=Error validating ISF {ISF_SOURCE_PATH}:\n{e}");
        std::process::exit(1);
    }
}
