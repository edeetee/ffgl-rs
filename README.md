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

### ISF Example

The following command will take an ISF file and output an FFGL plugin. The plugin will be copied to the resolume plugin folder.

`isf_to_resolume.sh <isf_file>`

### Commands

#### Deploy

The following script will turn the dylib into a macos bundle and copy it to the resolume plugin folder.
`./deploy_bundle_to_resolume.sh`

#### Run

`./run_resolume.sh`

#### Change log level

I have integrated [tracing](https://docs.rs/tracing/latest/tracing/index.html) into the plugin. To change the log level you can set the `RUST_LOG` environment variable. For example to set the log level to trace you can run

`RUST_LOG=trace ./run_resolume.sh`

## Future work

- Get working on Windows
- Get working on FFGL1 (VDMX)
- Embed any linked photos into the dylib along with the ISf source
- Handle multiple passes

## Aims

I want a simple wrapper to make plugings for VJ programs such as resolume and present the user a basic GL context that can be used however you want for fun &advanced FX/Sources. This could be a good starting point for other connections (Connect to a touchdesigner instance that can automatically pause and swap between COMPs)
