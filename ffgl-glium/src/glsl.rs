use build_common::GlslVersion;

/// Try get the best transpilation target for the given capabilities.
pub fn get_best_transpilation_target(ctx: &impl glium::CapabilitiesSource) -> Option<GlslVersion> {
    let glsl_versions = &ctx.get_capabilities().supported_glsl_versions;

    if glsl_versions
        .iter()
        .any(|v| matches!(v, glium::Version(glium::Api::Gl, 1, 4)))
    {
        Some(GlslVersion::Glsl140)
    } else if glsl_versions
        .iter()
        .any(|v| matches!(v, glium::Version(glium::Api::Gl, 1, 2)))
    {
        Some(GlslVersion::Glsl120)
    } else {
        None
    }
}
