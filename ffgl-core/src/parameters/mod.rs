//!Structs and enums for FFGL parameters
//! Use [info::SimpleParamInfo] for most simple instances
//! Implement [info::ParamInfo] yourself for more complex cases (see [example-isf/src/param.rs])

pub mod builtin;
pub mod handler;
mod info;
pub use info::*;
