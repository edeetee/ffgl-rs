use bindgen::Bindings;
use std::env;
use std::path::Path;
use std::path::PathBuf;

fn main() {
    println!("cargo:rerun-if-env-changed=BINDGEN_EXTRA_CLANG_ARGS");

    let clang_args_ffgl = vec![
        "-x".to_string(),
        "c++".to_string(),
        "-IFFGLSDK/Include".to_string(),
    ];

    let clang_args_ffgl2 = vec![
        "-x".to_string(),
        "c++".to_string(),
        "-Iffgl-resolume/source/lib/ffgl".to_string(),
    ];

    let out_dir = PathBuf::from(env::var("OUT_DIR").expect("env variable OUT_DIR not found"));

    // Generate the bindings.
    build_to_out_file(
        bindgen::Builder::default()
            .clang_args(&clang_args_ffgl)
            .header("wrapper.h")
            .generate()
            .unwrap(),
        &out_dir.join("ffgl.rs"),
    );

    build_to_out_file(
        bindgen::Builder::default()
            .clang_args(&clang_args_ffgl2)
            .header("wrapper.h")
            .generate()
            .unwrap(),
        &out_dir.join("ffgl2.rs"),
    );
}

fn build_to_out_file(bindings: Bindings, file: &Path) {
    // Write them to the crate root.
    bindings
        .write_to_file(file)
        .expect("could not write bindings");
}
