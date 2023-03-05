# FFGL in Rust

Here is the start a rust binding for FFGL. I reference both the old and resolume versions of FFGL with the aim to support both on all platforms.

## Functionality

- Bindings for FFGL1.x + 2.x
- Logging for resolume
- Conversions between FFI and Rust types
- Basic logic, pointer casting and handling to successfully launch in resolume.

## Aims

I want a simple wrapper to make plugings for VJ programs such as resolume and present the user a basic GL context that can be used however you want for fun  &advanced FX/Sources. This could be a good starting point for other connections (Connect to a touchdesigner instance that can automatically pause and swap between COMPs)