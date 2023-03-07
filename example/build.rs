use std::env;
use std::fs::File;
use std::path::Path;
use std::path::PathBuf;

use gl_generator::Api;
use gl_generator::Fallbacks;
use gl_generator::GlobalGenerator;
use gl_generator::Profile;
use gl_generator::Registry;
use gl_generator::StaticGenerator;

fn main() {
    let out_dir = PathBuf::from(env::var("OUT_DIR").expect("env variable OUT_DIR not found"));
    let mut file = File::create(&Path::new(&out_dir).join("gl.rs")).unwrap();

    Registry::new(Api::Gl, (4, 6), Profile::Core, Fallbacks::All, [])
        .write_bindings(GlobalGenerator, &mut file)
        .unwrap();
}
