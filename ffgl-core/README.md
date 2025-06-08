# ffgl-core

`ffgl-core` is a Rust crate for interacting with FFGL plugins.

## Building on Windows

This crate aims to support compilation on Windows. To facilitate this, the `build.rs` script has been configured to:
- Link with `opengl32.lib`, which is necessary for OpenGL applications on Windows.
- Set up `bindgen` with the appropriate clang arguments for the Windows target, including the `-lopengl32` linker flag.

**Prerequisites for Building on Windows:**

To successfully build `ffgl-core` on a Windows system, you will need:
1.  **A Rust Windows toolchain**: For example, `x86_64-pc-windows-msvc` or `x86_64-pc-windows-gnu`. You can install this via `rustup target add <your-target>`.
2.  **A C++ toolchain compatible with `clang`**:
    *   **MSVC**: If you are using the `msvc` Rust toolchain, you'll need the Visual Studio Build Tools with the C++ toolset installed. `clang` can often use these.
    *   **MinGW-w64**: If you are using the `gnu` Rust toolchain (e.g., `x86_64-pc-windows-gnu`), you'll need a MinGW-w64 toolchain that provides the necessary headers and libraries (like `opengl32.lib` and `windows.h`).
3.  **Windows SDK**: Essential system headers like `windows.h` must be available and discoverable by `clang` (which `bindgen` uses under the hood). These typically come with the Visual Studio Build Tools or a comprehensive MinGW-w64 distribution.
4.  **Clang**: `bindgen` relies on `libclang`. Ensure `clang` is installed and accessible in your PATH, or that `LIBCLANG_PATH` is set.

**Verification Note:**

While the build script has been updated for Windows compatibility, the successful compilation on a Windows environment could not be fully verified by the assistant due to limitations of the testing environment. Users attempting to build on Windows should ensure all the prerequisites mentioned above are met.
