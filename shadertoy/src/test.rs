use super::*;
#[cfg(test)]
use serde_json::{Value, json};

const TEST_SHADERTOY_RESPONSE: &str = r##"{
    "Shader": {
      "ver": "0.1",
      "info": {
        "id": "3dlfWH",
        "date": "1588367654",
        "viewed": 1257,
        "name": "Gentle Rings",
        "username": "darknoon",
        "description": "Just a simple relaxing sine combination",
        "likes": 16,
        "published": 3,
        "flags": 0,
        "usePreview": 0,
        "tags": [
          "rings"
        ],
        "hasliked": 0
      },
      "renderpass": [
        {
          "inputs": [],
          "outputs": [
            {
              "id": 37,
              "channel": 0
            }
          ],
          "code": "#define PI 3.141596\n\n\nvec3 a = vec3(0.5, 0.5, 0.5);\nvec3 b = vec3(0.5, 0.5, 0.5);\nvec3 c = vec3(1.0, 1.0, 1.0);\nvec3 d = vec3(0.00, 0.33, 0.67);\n\n// iq color mapper\nvec3 colorMap(float t) {\n\treturn (a + b * cos(2. * PI * (c * t + d)));\n}\n\nvoid mainImage(out vec4 o, in vec2 i)\n{\n    vec2 uv = i / iResolution.xy;\n    uv -= 0.5;\n    uv.x *= iResolution.x / iResolution.y;\n    \n    float r = length(uv);\n    float a = atan(uv.y, uv.x);\n    \n    float ring = 1.5 + 0.8 * sin(PI * 0.25 * iTime);\n    \n    float kr = 0.5 - 0.5 * cos(7. * PI * r); \n    vec3 kq = 0.5 - 0.5 * sin(ring*vec3(30., 29.3, 28.6) * r - 6.0 * iTime + PI * vec3(-0.05, 0.5, 1.0));\n    vec3 c = kr * (0.1 + kq * (1. - 0.5* colorMap(a / PI))) * (0.5 + 0.5 * sin(11.*a + 22.5*r));\n\n    // Output to screen\n    o.rgb = mix(vec3(0.0, 0.0, 0.2), c, 0.85);\n}",
          "name": "Image",
          "description": "",
          "type": "image"
        }
      ]
    }
  }"##;

const TEST_SHADERTOY_RESPONSE_ISF: &str = r##"/*{
  "DESCRIPTION": "Just a simple relaxing sine combination",
  "CREDIT": "Converted from Shadertoy: Gentle Rings",
  "CATEGORIES": [
    "Generators"
  ],
  "INPUTS": [
    {
      "NAME": "progress",
      "TYPE": "float",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    }
  ]
}
*/

#define PI 3.141596


vec3 a = vec3(0.5, 0.5, 0.5);
vec3 b = vec3(0.5, 0.5, 0.5);
vec3 c = vec3(1.0, 1.0, 1.0);
vec3 d = vec3(0.00, 0.33, 0.67);

// iq color mapper
vec3 colorMap(float t) {
        return (a + b * cos(2. * PI * (c * t + d)));
}

void mainImage(out vec4 o, in vec2 i)
{
    vec2 uv = i / RENDERSIZE.xy;
    uv -= 0.5;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;
    
    float r = length(uv);
    float a = atan(uv.y, uv.x);
    
    float ring = 1.5 + 0.8 * sin(PI * 0.25 * (TIME + 10.0 * progress));
    
    float kr = 0.5 - 0.5 * cos(7. * PI * r); 
    vec3 kq = 0.5 - 0.5 * sin(ring*vec3(30., 29.3, 28.6) * r - 6.0 * (TIME + 10.0 * progress) + PI * vec3(-0.05, 0.5, 1.0));
    vec3 c = kr * (0.1 + kq * (1. - 0.5* colorMap(a / PI))) * (0.5 + 0.5 * sin(11.*a + 22.5*r));

    // Output to screen
    o.rgb = mix(vec3(0.0, 0.0, 0.2), c, 0.85);
}

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}"##;

#[test]
fn parse_shadertoy_response() {
    serde_json::from_str::<ShadertoyResponse>(TEST_SHADERTOY_RESPONSE)
        .expect("Failed to parse Shadertoy response");
}

#[test]
fn parse_and_convert_response() {
    let response: ShadertoyResponse =
        serde_json::from_str(TEST_SHADERTOY_RESPONSE).expect("Failed to parse Shadertoy response");

    let simplified: SimplifiedShader = response.Shader.into();

    // Convert the shader code to ISF format
    let isf_code = convert_shadertoy_to_isf(&simplified).expect("Failed to convert shader to ISF");

    // Check if the conversion was successful
    assert_eq!(isf_code, TEST_SHADERTOY_RESPONSE_ISF);
}

#[test]
fn test_extract_shadertoy_id() {
    let url = "https://www.shadertoy.com/view/XsXXXX";
    let result = extract_shadertoy_id(url);
    assert!(result.is_ok());
    assert_eq!(result.unwrap(), "XsXXXX");

    let invalid_url = "https://example.com";
    let result = extract_shadertoy_id(invalid_url);
    assert!(result.is_err());
}

#[test]
fn test_transform_shadertoy_code() {
    let shadertoy_code = r#"
            void mainImage( out vec4 fragColor, in vec2 fragCoord )
            {
                vec2 uv = fragCoord / iResolution.xy;
                float t = iTime * 0.5;
                vec3 color = 0.5 + 0.5 * cos( t + uv.xyx + vec3(0,2,4) );
                fragColor = vec4(color, 1.0);
            }
        "#;

    let result = transform_shadertoy_code(shadertoy_code);
    assert!(result.is_ok());

    let transformed = result.unwrap();

    // Verify main transformation
    assert!(transformed.contains("void main()"));
    assert!(!transformed.contains("void mainImage"));

    // Verify variable transformations
    assert!(transformed.contains("gl_FragColor"));
    assert!(!transformed.contains("fragColor"));

    assert!(transformed.contains("gl_FragCoord.xy"));
    assert!(!transformed.contains("fragCoord"));

    assert!(transformed.contains("RENDERSIZE"));
    assert!(!transformed.contains("iResolution"));

    assert!(transformed.contains("(TIME + 10.0 * progress)"));
    assert!(!transformed.contains("iTime"));
}

#[test]
fn test_detect_shadertoy_parameters() {
    let code = r#"
            float speed = 1.5;
            float intensity = 0.8;
            
            void mainImage( out vec4 fragColor, in vec2 fragCoord )
            {
                // Some code
            }
        "#;

    let result = detect_shadertoy_parameters(code);
    assert!(result.is_ok());

    let params = result.unwrap();
    assert_eq!(params.len(), 2);

    // Check that speed and intensity were detected
    let has_speed = params.iter().any(|p| p.name == "speed");
    let has_intensity = params.iter().any(|p| p.name == "intensity");

    assert!(has_speed);
    assert!(has_intensity);
}

// This is a more realistic test with a complete Shadertoy shader
#[test]
fn test_convert_complete_shader() {
    let shadertoy = SimplifiedShader {
        code: r#"
            // A simple Shadertoy example
            float speed = 0.5;
            
            void mainImage( out vec4 fragColor, in vec2 fragCoord )
            {
                vec2 uv = fragCoord / iResolution.xy;
                float t = iTime * speed;
                
                vec3 color = 0.5 + 0.5 * cos( t + uv.xyx + vec3(0,2,4) );
                
                fragColor = vec4(color, 1.0);
            }
            "#
        .to_string(),
        name: "Rainbow Shader".to_string(),
        description: Some("A simple rainbow shader".to_string()),
        inputs: vec![],
    };

    let result = convert_shadertoy_to_isf(&shadertoy);
    assert!(result.is_ok());

    let isf = result.unwrap();

    // Check ISF metadata is present
    assert!(isf.contains("/*"));
    assert!(isf.contains("*/"));
    assert!(isf.contains("\"description\""));
    assert!(isf.contains("\"inputs\""));

    // Check for progress parameter in metadata
    assert!(isf.contains("\"progress\""));

    // Check code transformation
    assert!(isf.contains("void main()"));
    assert!(isf.contains("gl_FragColor"));
    assert!(isf.contains("RENDERSIZE"));
    assert!(isf.contains("(TIME + 10.0 * progress)"));
}
