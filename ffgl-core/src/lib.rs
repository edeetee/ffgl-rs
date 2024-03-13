//! # FFGL plugin
//!
//! This crate provides a set of tools to create FFGL plugins in Rust.
//!
//! FFGL Plugins require a plugMain function to be defined for the host to call.
//! the [plugin_main] macro will generate this function for you.
//!
//! The quickest way to get started is to implement the [handler::simplified::SimpleFFGLInstance] trait on a struct that represents your plugin instance.
//!
//! Then, call ```plugin_main!(SimpleFFGLHandler<YourSimpleFFGLInstanceStruct>)``` to generate the plugMain function.
//!
//! If you want to control the details of plugin instantiation, see [handler].
//!
//! # Running
//!
//! To actually run your plugin, you'll need to copy it to the FFGL plugin directory.
//! On macos, you will need to additionally package it as a bundle.
//! There are helper scripts in the repository that can assist you with this (./deploy_bundle.sh)
//!
//! ## Support
//!
//! If you have any questions, feel free to send me an email at [dev@edt.nz](mailto:dev@edt.nz)
//!
//! Feel free to get involved in the repo at [github.com/edeetee/ffgl-rs](github.com/edeetee/ffgl-rs)

pub mod conversions;
pub mod entry;
pub mod ffi;
pub mod info;
pub mod inputs;
pub mod log;

pub mod handler;
pub mod handler_macro;

pub mod parameters;

pub use inputs::*;

pub use tracing;
