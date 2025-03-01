This respository holds a rust ffgl plugin framework for resolume.
It also holds a plugin that takes an ISF shader and compiles it as a plugin.

Assume that .fs files are ISF shaders and .rs files are rust files.

When making animating shaders, try to use a progress variable that goes from 0 to 1. This will allow the shader to be animated in resolume by manipulating the progress variable.
