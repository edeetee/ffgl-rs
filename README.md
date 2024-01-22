# FFGL in Rust

Here is the start a rust binding for FFGL. I reference both the old and resolume versions of FFGL with the aim to support both on all platforms.

## Support

- Currently Macos only
  - Wouldn't require too much to get working on windows, just need to get the build script working
- FFGL 2 (resolume)

## Functionality

- Logging inside resolume
- Handy scripts to build and run inside resolume
- Example ISF plugin
  - Give an input ISF file and output an FFGL plugin
  - Quicker than using Wire for basic ISF shaders

## Usage

Currently only tested on macos.

### Build

``

## Future work

- Get working on Windows
- Get working on FFGL1 (VDMX)

## Aims

I want a simple wrapper to make plugings for VJ programs such as resolume and present the user a basic GL context that can be used however you want for fun &advanced FX/Sources. This could be a good starting point for other connections (Connect to a touchdesigner instance that can automatically pause and swap between COMPs)
