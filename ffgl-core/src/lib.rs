//! # FFGL plugin
//!
//! This crate provides a set of tools to create FFGL plugins in Rust.
//!
//! FFGL Plugins require a plugMain function to be defined for the host to call.
//! the [ffgl_handler] macro will generate this function for you.
//!
//! The quickest way to get started is to implement the [handler::simplified::SimpleFFGLInstance] trait on a struct that represents your plugin instance.
//!
//! Then, call ```ffgl_handler!(SimpleFFGLHandler<YourSimpleFFGLInstanceStruct>)``` to generate the plugMain function.
//!
//! If you want to control the details of plugin instantiation, see [handler].
//!
//! ## Support
//!
//! If you have any questions, feel free to send me an email at [dev@edt.nz](mailto:dev@edt.nz)
//!
//! Feel free to get involved in the repo at [github.com/edeetee/ffgl-rs](github.com/edeetee/ffgl-rs)

pub mod conversions;
pub mod entry;
pub mod ffi;
mod handler_macro;
pub mod info;
pub mod inputs;
mod instance;
pub mod log;

pub mod handler;
pub mod parameters;

pub use inputs::*;

pub use tracing;
