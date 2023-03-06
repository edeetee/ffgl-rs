#![allow(clashing_extern_declarations)]

#[cfg(target_os = "macos")]
#[link(name = "OpenGL", kind = "framework")]
extern "C" {}

#[cfg(target_os = "linux")]
#[link(name = "GL")]
extern "C" {}

include!(concat!(env!("OUT_DIR"), "/gl.rs"));