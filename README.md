# FFGL in Rust

Here is the start a rust binding for FFGL. I reference both the old and resolume versions of FFGL with the aim to support both on all platforms.

## Functionality

- Bindings for FFGL1.x + 2.x
- Logging for resolume
- Conversions between FFI and Rust types
- Basic logic, pointer casting and handling to successfully launch in resolume.

## Usage
Currently only tested on macos. I just run ./run.sh and it builds the build cdylib to a macos bundle, runs resolume and listens to the log file.

In order to do this yourself, you'll have to make a macos bundle in the same location (you can copy one from the resolume repo I think....) To do this automatically, we need a crate that can do this for all platforms.

## Aims

I want a simple wrapper to make plugings for VJ programs such as resolume and present the user a basic GL context that can be used however you want for fun  &advanced FX/Sources. This could be a good starting point for other connections (Connect to a touchdesigner instance that can automatically pause and swap between COMPs)