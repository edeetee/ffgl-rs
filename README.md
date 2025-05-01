# FFGL in Rust

FFGL (Resolume & VDMX plugin) framework for rust.

![Screenshot](docs/screenshot.png)

## Support

- ! Currently Macos only
  - Wouldn't require too much to get working on windows, someone just needs to test and create the build script
- FFGL 2 (resolume)
- VDMX 5 (Currently borked)
- ISF Shader support

Please get in contact with me at [vis@edt.nz](mailto:vis@edt.nz) if you have any questions!

## Functionality

- Logging inside resolume
- Handy scripts to build and run inside resolume
- Example ISF plugin
  - Give an input ISF file and output an FFGL plugin
  - Quicker than using Wire for basic ISF shaders
- Compile error for invalid ISF code
- GLSL translation

## Usage

### ISF Example

The following command will take an ISF file, compile it to a plugin and deploy it to the system plugin folder. It is a good example of the steps required to make a plugin work.

`ffgl-isf/deploy_isf.sh <isf_file>`

#### Bulk script

There also a bulk version of the script that will deploy all the ISF files in the example_isf folder and some from the system ISF directory.
`ffgl-isf/bulk_deploy_isf.sh`
Add the `-e` flag to only print the compilation errors.

#### Debug & iterate script

`ffgl_run_resolume.sh <isf_file\name>`

This script intelligently finds the isf file you're talking about and runs it.
It will automatically add RUST_LOG parameters to filter for the specific isf file you're working on.

### Commands

#### Deploy

Once a plugin has been built, it needs to be deployed to the system FFGL directory. On macos, it needs to be packaged as a 'Bundle' (A fancy folder).
`deploy_bundle.sh <output_lib_name>`

#### Run

`./resolume.sh`
or
`./vdmx.sh`

#### Change log level

I have integrated [tracing](https://docs.rs/tracing/latest/tracing/index.html) into the plugin. To change the log level you can set the `RUST_LOG` environment variable. For example to set the log level to trace you can run

`RUST_LOG=trace ./resolume.sh`

## Extending

### ffgl-core

You can create your own plugin from scratch by either using the SimpleFFGLInstance trait, or implement both the FFGLHandler and FFGLInstance traits.

You must call the ffgl_handler!() macro to associate your plugin with the correct entry points.

### ffgl-glium

Use this to create a glium instance while inside an ffgl plugin

### ffgl-isf

Use this to create an isf plugin. Needs more work to be modular.

## Future work

- Get working on Windows
- Embed any linked photos into the dylib along with the ISf source
- Handle multiple passes ✔️
- Label inputs
- String inputs

## Aims

I want a simple wrapper to make plugins for VJ programs such as resolume and present the user a basic GL context that can be used however you want for fun &advanced FX/Sources. This could be a good starting point for other connections (Connect to a touchdesigner instance that can automatically pause and swap between COMPs)
