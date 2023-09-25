use gl;

pub unsafe fn validate_context_state() {
    let mut gl_int: i32 = 0;

    // Check if the current shader program is unbound.
    gl::GetIntegerv(gl::CURRENT_PROGRAM, &mut gl_int);
    assert_eq!(gl_int, 0, "Current shader program should be unbound.");

    // Check if the active texture unit is set to the default (TEXTURE0).
    gl::GetIntegerv(gl::ACTIVE_TEXTURE, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::TEXTURE0 as i32,
        "Active texture unit should be set to TEXTURE0."
    );

    struct TextureType {
        target: u32,
        binding: u32,
    }
    const TEXTURE_TYPES: [TextureType; 2] = [
        TextureType {
            target: gl::TEXTURE_1D,
            binding: gl::TEXTURE_BINDING_1D,
        },
        TextureType {
            target: gl::TEXTURE_2D,
            binding: gl::TEXTURE_BINDING_2D,
        },
        // Add other texture types here...
    ];

    let mut num_samplers: i32 = 0;
    gl::GetIntegerv(gl::MAX_TEXTURE_IMAGE_UNITS, &mut num_samplers);
    for texture_type in TEXTURE_TYPES.iter() {
        for sampler in 0..num_samplers {
            gl::ActiveTexture(gl::TEXTURE0 + sampler as u32);
            // Check if textures are unbound for the current texture unit.
            gl::GetIntegerv(texture_type.binding, &mut gl_int);
            assert_eq!(
                gl_int, 0,
                "Texture should be unbound for texture unit {}.",
                sampler
            );
        }
    }
    gl::ActiveTexture(gl::TEXTURE0);

    // Check if the vertex buffer object (VBO) is unbound.
    gl::GetIntegerv(gl::ARRAY_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Vertex buffer object (VBO) should be unbound.");

    // Unbind other uncommonly used bindings (optional).
    //gl::GetIntegerv(gl::COPY_READ_BUFFER_BINDING, &mut gl_int);
    //assert_eq!(gl_int[0], 0);
    //gl::GetIntegerv(gl::COPY_WRITE_BUFFER_BINDING, &mut gl_int);
    //assert_eq!(gl_int[0], 0);
    gl::GetIntegerv(gl::DRAW_INDIRECT_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Draw indirect buffer should be unbound.");
    gl::GetIntegerv(gl::PIXEL_PACK_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Pixel pack buffer should be unbound.");
    gl::GetIntegerv(gl::PIXEL_UNPACK_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Pixel unpack buffer should be unbound.");
    gl::GetIntegerv(gl::TRANSFORM_FEEDBACK_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Transform feedback buffer should be unbound.");

    // Check if the index buffer object (IBO) is unbound.
    gl::GetIntegerv(gl::ELEMENT_ARRAY_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Index buffer object (IBO) should be unbound.");

    // Check if the uniform buffer object (UBO) is unbound.
    gl::GetIntegerv(gl::UNIFORM_BUFFER_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Uniform buffer object (UBO) should be unbound.");

    // Check if the vertex array object (VAO) is unbound.
    gl::GetIntegerv(gl::VERTEX_ARRAY_BINDING, &mut gl_int);
    assert_eq!(gl_int, 0, "Vertex array object (VAO) should be unbound.");

    // Check various render state settings.
    gl::GetIntegerv(gl::POLYGON_MODE, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::FILL as i32,
        "Polygon mode should be set to FILL."
    );

    assert_eq!(
        gl::IsEnabled(gl::CULL_FACE),
        gl::FALSE,
        "Cull face should be disabled."
    );
    gl::GetIntegerv(gl::FRONT_FACE, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::CCW as i32,
        "Front face should be set to CCW (Counter Clockwise)."
    );

    assert_eq!(
        gl::IsEnabled(gl::BLEND),
        gl::FALSE,
        "Blending should be disabled."
    );

    gl::GetIntegerv(gl::BLEND_EQUATION_RGB, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::FUNC_ADD as i32,
        "RGB blend equation should be set to FUNC_ADD."
    );
    gl::GetIntegerv(gl::BLEND_EQUATION_ALPHA, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::FUNC_ADD as i32,
        "Alpha blend equation should be set to FUNC_ADD."
    );

    gl::GetIntegerv(gl::BLEND_SRC_RGB, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::ONE as i32,
        "RGB source blend factor should be set to ONE."
    );
    gl::GetIntegerv(gl::BLEND_SRC_ALPHA, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::ONE as i32,
        "Alpha source blend factor should be set to ONE."
    );
    gl::GetIntegerv(gl::BLEND_DST_RGB, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::ZERO as i32,
        "RGB destination blend factor should be set to ZERO."
    );
    gl::GetIntegerv(gl::BLEND_DST_ALPHA, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::ZERO as i32,
        "Alpha destination blend factor should be set to ZERO."
    );

    let mut gl_bool_depth_mask: u8 = 0;
    gl::GetBooleanv(gl::DEPTH_WRITEMASK, &mut gl_bool_depth_mask);
    assert_eq!(
        gl_bool_depth_mask,
        gl::TRUE as u8,
        "Depth write mask should be set to TRUE."
    );

    assert_eq!(
        gl::IsEnabled(gl::DEPTH_TEST),
        gl::FALSE,
        "Depth test should be disabled."
    );
    gl::GetIntegerv(gl::DEPTH_FUNC, &mut gl_int);
    assert_eq!(
        gl_int,
        gl::LESS as i32,
        "Depth function should be set to LESS."
    );

    let mut gl_bool_color_mask: [u8; 4] = [0; 4];
    gl::GetBooleanv(gl::COLOR_WRITEMASK, &mut gl_bool_color_mask[0]);
    assert_eq!(
        gl_bool_color_mask[0],
        gl::TRUE as u8,
        "Red color write mask should be set to TRUE."
    );
    assert_eq!(
        gl_bool_color_mask[1],
        gl::TRUE as u8,
        "Green color write mask should be set to TRUE."
    );
    assert_eq!(
        gl_bool_color_mask[2],
        gl::TRUE as u8,
        "Blue color write mask should be set to TRUE."
    );
    assert_eq!(
        gl_bool_color_mask[3],
        gl::TRUE as u8,
        "Alpha color write mask should be set to TRUE."
    );
}
