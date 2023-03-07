use std::env;
use std::fs::File;
use std::path::PathBuf;
use std::path::Path;

use bindgen::Bindings;

//shamelessly stolen from https://github.com/simlay/uikit-sys/blob/master/build.rs

fn sdk_path(sdk: &str) -> Result<String, std::io::Error> {
    use std::process::Command;

    let output = Command::new("xcrun")
        .args(&["--sdk", sdk, "--show-sdk-path"])
        .output()?
        .stdout;
    let prefix_str = std::str::from_utf8(&output).expect("invalid output from `xcrun`");
    Ok(prefix_str.trim_end().to_string())
}

fn main() {
    // Generate one large set of bindings for all frameworks.
    //
    // We do this rather than generating a module per framework as some frameworks depend on other
    // frameworks and in turn share types. To ensure all types are compatible across each
    // framework, we feed all headers to bindgen at once.
    //
    // Only link to each framework and include their headers if their features are enabled and they
    // are available on the target os.
    println!("cargo:rerun-if-env-changed=BINDGEN_EXTRA_CLANG_ARGS");

    let sdk_dir = PathBuf::from(sdk_path("macosx").unwrap());
    let frameworks_path = PathBuf::from("System/Library/Frameworks/");
    let gl_path = PathBuf::from("System/Library/Frameworks/OpenGL.framework/Headers/");

    dbg!(&sdk_dir, &gl_path);

    let full_gl_path = sdk_dir.join(gl_path);
    let full_gl_path_str = full_gl_path.to_str().unwrap();

    let full_frameworks_path = sdk_dir.join(frameworks_path);
    let full_frameworks_path = full_frameworks_path.to_str().unwrap();

    // Begin building the bindgen params.
    // let mut builder = ;

    let clang_args = vec![
        format!("-F{full_frameworks_path}"),
        "-x".to_string(), "c++".to_string(),
        // "-Iffgl-resolume/source/".to_string(),
        "-IFFGLSDK/Include".to_string()
    ];

    let clang_args_ffgl2 = vec![
        format!("-F{full_frameworks_path}"),
        "-x".to_string(), "c++".to_string(),
        "-Iffgl-resolume/source/lib/ffgl".to_string(),
        // "-IFFGLSDK/Include".to_string()
    ]; 

    dbg!(&clang_args);
    dbg!(&clang_args_ffgl2);

    // builder = ;

    let out_dir = PathBuf::from(env::var("OUT_DIR").expect("env variable OUT_DIR not found"));

    // Generate the bindings.
    build_to_out_file(
        bindgen::Builder::default()
            .clang_args(&clang_args)
            .header("wrapper.h")
            .generate().unwrap(),
        &out_dir.join("ffgl.rs")
    );

    build_to_out_file(
        bindgen::Builder::default()
            .clang_args(&clang_args_ffgl2)
            .header("wrapper.h")
            .generate().unwrap(),
        &out_dir.join("ffgl2.rs")
    );
}


fn build_to_out_file(bindings: Bindings, file: &Path) {
    // let bindings = builder.generate().expect("unable to generate bindings");

    // Get the cargo out directory.

    // Write them to the crate root.
    bindings
        .write_to_file(file)
        .expect("could not write bindings");
}