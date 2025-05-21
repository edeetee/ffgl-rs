use bindgen::Bindings;
use std::env;
use std::path::Path;
use std::path::PathBuf;

/// use xcrun to find the path to the MacOS SDK
fn macos_get_framework_sdk_path() -> String {
    let output = std::process::Command::new("xcrun")
        .args(&["--show-sdk-path"])
        .output()
        .expect("failed to execute xcrun");

    let path = String::from_utf8(output.stdout).expect("failed to parse xcrun output");
    path.trim().to_string() + "/System/Library/Frameworks"
}

fn main() {
    println!("cargo:rerun-if-env-changed=BINDGEN_EXTRA_CLANG_ARGS");

    let mut clang_args_ffgl = vec!["-x", "c++", "-IFFGLSDK/Include"];
    let mut clang_args_ffgl2 = vec!["-x", "c++", "-Iffgl-resolume/source/lib/ffgl"];

    cfg_if::cfg_if! {
        if #[cfg(target_os = "macos")] {
            let macos_framework_path = macos_get_framework_sdk_path();
            let extra_clang_args = vec!["-F", &macos_framework_path, "-framework", "OpenGL"];

            clang_args_ffgl.extend(&extra_clang_args);
            clang_args_ffgl2.extend(&extra_clang_args);
        } else if #[cfg(target_os = "windows")] {
            println!("cargo:rustc-link-lib=opengl32");
            // Add linker argument for bindgen
            let extra_clang_args = vec!["-lopengl32"];
            clang_args_ffgl.extend(&extra_clang_args);
            clang_args_ffgl2.extend(&extra_clang_args);
        }
    }

    dbg!(&clang_args_ffgl);
    dbg!(&clang_args_ffgl2);

    let out_dir = PathBuf::from(env::var("OUT_DIR").expect("env variable OUT_DIR not found"));

    // Generate the bindings.
    build_to_out_file(
        bindgen::Builder::default()
            .clang_args(&clang_args_ffgl)
            .header("wrapper.h")
            .generate()
            .unwrap(),
        &out_dir.join("ffgl1.rs"),
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
