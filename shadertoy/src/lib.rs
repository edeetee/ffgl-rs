use regex::Regex;
use reqwest;
use serde::{Deserialize, Serialize};
use serde_json::Value;
use std::error::Error;

#[cfg(test)]
mod test;

/// Represents the complete structure of a Shadertoy API response
#[derive(Debug, Deserialize, Clone)]
pub struct ShadertoyResponse {
    pub Shader: ShadertoyShader,
}

/// Represents the structure of a Shadertoy shader
#[derive(Debug, Deserialize, Clone)]
pub struct ShadertoyShader {
    pub ver: String,
    pub info: ShaderInfo,
    pub renderpass: Vec<RenderPass>,
}

/// Represents the info section of a shader
#[derive(Debug, Deserialize, Clone)]
pub struct ShaderInfo {
    pub id: String,
    pub name: String,
    pub description: String,
    pub username: String,
    pub tags: Vec<String>,
    // Other fields like date, viewed, etc. could be added as needed
}

/// Represents a render pass in the shader
#[derive(Debug, Deserialize, Clone)]
pub struct RenderPass {
    pub inputs: Vec<ShadertoyInput>,
    pub outputs: Vec<ShadertoyOutput>,
    pub code: String,
    pub name: String,
    pub description: String,
    #[serde(rename = "type")]
    pub pass_type: String,
}

/// Represents an input used in a Shadertoy shader
#[derive(Debug, Deserialize, Clone)]
pub struct ShadertoyInput {
    pub channel: i32,
    #[serde(rename = "type")]
    pub input_type: Option<String>, // texture, cubemap, etc.
}

/// Represents an output in a Shadertoy render pass
#[derive(Debug, Deserialize, Clone)]
pub struct ShadertoyOutput {
    pub id: i32,
    pub channel: i32,
}

/// Simplified ShadertoyShader struct for internal use
#[derive(Debug, Clone)]
pub struct SimplifiedShader {
    pub code: String,
    pub name: String,
    pub description: Option<String>,
    pub inputs: Vec<ShadertoyInput>,
}

impl From<ShadertoyShader> for SimplifiedShader {
    fn from(shader: ShadertoyShader) -> Self {
        SimplifiedShader {
            code: shader
                .renderpass
                .iter()
                .map(|rp| rp.code.clone())
                .collect::<Vec<_>>()
                .join("\n"),
            name: shader.info.name,
            description: Some(shader.info.description),
            inputs: shader
                .renderpass
                .iter()
                .flat_map(|rp| rp.inputs.clone())
                .collect(),
        }
    }
}

/// Represents ISF shader metadata
#[derive(Debug, Serialize)]
#[serde(rename_all = "UPPERCASE")]
pub struct IsfMetadata {
    pub description: String,
    pub credit: Option<String>,
    pub categories: Vec<String>,
    pub inputs: Vec<IsfInput>,
}

/// Represents an input parameter for ISF
#[derive(Debug, Serialize)]
#[serde(rename_all = "UPPERCASE")]
pub struct IsfInput {
    pub name: String,
    pub r#type: String,
    pub default: f32,
    pub min: Option<f32>,
    pub max: Option<f32>,
}

/// Error types for shadertoy conversion process
#[derive(Debug)]
pub enum ShadertoyConversionError {
    FetchError(reqwest::Error),
    ParsingError(String),
    ConversionError(String),
}

impl std::fmt::Display for ShadertoyConversionError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            ShadertoyConversionError::FetchError(e) => write!(f, "Failed to fetch shader: {}", e),
            ShadertoyConversionError::ParsingError(e) => write!(f, "Failed to parse shader: {}", e),
            ShadertoyConversionError::ConversionError(e) => {
                write!(f, "Failed to convert shader: {}", e)
            }
        }
    }
}

impl Error for ShadertoyConversionError {}

/// Convert a Shadertoy URL to an ISF shader string
pub async fn convert_shadertoy_url(
    url: &str,
    api_key: &str,
) -> Result<String, ShadertoyConversionError> {
    // Extract the shader ID from the URL
    let shader_id = extract_shadertoy_id(url)?;

    // Fetch the shader from the Shadertoy API
    let shader = fetch_shadertoy_shader(&shader_id, api_key).await?;

    // Convert the Shadertoy shader to ISF format
    let isf_shader = convert_shadertoy_to_isf(&shader)?;

    Ok(isf_shader)
}

/// Extract the Shadertoy ID from a URL
pub fn extract_shadertoy_id(url: &str) -> Result<String, ShadertoyConversionError> {
    // Regex to match Shadertoy URLs like https://www.shadertoy.com/view/XsXXXX
    let re = Regex::new(r"shadertoy\.com/view/(\w+)").unwrap();

    if let Some(captures) = re.captures(url) {
        if let Some(id) = captures.get(1) {
            return Ok(id.as_str().to_string());
        }
    }

    Err(ShadertoyConversionError::ParsingError(
        "Could not extract shader ID from URL".to_string(),
    ))
}

/// Fetch a Shadertoy shader by ID from the Shadertoy API
async fn fetch_shadertoy_shader(
    id: &str,
    api_key: &str,
) -> Result<SimplifiedShader, ShadertoyConversionError> {
    let url = format!("https://www.shadertoy.com/api/v1/shaders/{id}?key={api_key}");

    println!("Fetching shader from URL: {}", url);

    let response = reqwest::get(&url)
        .await
        .map_err(ShadertoyConversionError::FetchError)?;

    let shadertoy_response: ShadertoyResponse = response
        .json()
        .await
        .map_err(ShadertoyConversionError::FetchError)?;

    let shader = shadertoy_response.Shader;

    Ok(shader.into())
}

/// Convert a Shadertoy shader to ISF format
pub fn convert_shadertoy_to_isf(
    shader: &SimplifiedShader,
) -> Result<String, ShadertoyConversionError> {
    // Create ISF metadata
    let isf_metadata = create_isf_metadata(shader)?;

    // Convert the actual shader code
    let isf_code = transform_shadertoy_code(&shader.code)?;

    // Combine metadata and code
    let isf_shader = format!(
        "/*{}\n*/\n\n{}",
        serde_json::to_string_pretty(&isf_metadata)
            .map_err(|e| ShadertoyConversionError::ConversionError(e.to_string()))?,
        isf_code
    );

    Ok(isf_shader)
}

/// Create ISF metadata from a Shadertoy shader
fn create_isf_metadata(shader: &SimplifiedShader) -> Result<IsfMetadata, ShadertoyConversionError> {
    // Default inputs for animation
    let mut inputs = vec![
        IsfInput {
            name: "progress".to_string(),
            r#type: "float".to_string(),
            default: 0.0,
            min: Some(0.0),
            max: Some(1.0),
        },
        IsfInput {
            name: "speed".to_string(),
            r#type: "float".to_string(),
            default: 1.0,
            min: Some(0.0),
            max: Some(5.0),
        },
    ];

    // Add additional detected parameters
    let detected_params = detect_shadertoy_parameters(&shader.code)?;
    inputs.extend(detected_params);

    Ok(IsfMetadata {
        description: shader
            .description
            .clone()
            .unwrap_or_else(|| shader.name.clone()),
        credit: Some(format!("Converted from Shadertoy: {}", shader.name)),
        categories: vec!["Generators".to_string()],
        inputs,
    })
}

const SHADERTOY_SUFFIX: &str = r#"

void main() {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}"#;

/// Transform Shadertoy shader code to ISF format
pub fn transform_shadertoy_code(code: &str) -> Result<String, ShadertoyConversionError> {
    let mut transformed = code.to_string();

    // Replace fragColor with gl_FragColor
    // transformed = transformed.replace("fragColor", "gl_FragColor");

    // Replace fragCoord with gl_FragCoord
    // transformed = transformed.replace("fragCoord", "gl_FragCoord.xy");

    // Replace iResolution with RENDERSIZE
    transformed = transformed.replace("iResolution", "RENDERSIZE");

    // Replace different time variables
    transformed = transformed.replace("iGlobalTime", "(TIME * speed + 10.0 * progress)");
    transformed = transformed.replace("iTime", "(TIME * speed + 10.0 * progress)");

    // Replace other common Shadertoy uniforms
    transformed = transformed.replace("iTimeDelta", "TIME_DELTA");
    transformed = transformed.replace("iFrame", "FRAMEINDEX");
    transformed = transformed.replace("iMouse", "vec4(mouse.xy, 0.0, 0.0)");

    // Handle texture references - replace iChannelX with appropriate ISF texture
    for i in 0..4 {
        transformed = transformed.replace(&format!("iChannel{}", i), &format!("inputImage{}", i));
        transformed = transformed.replace(
            &format!("iChannelResolution[{}]", i),
            &format!("IMG_SIZE(iChannel{})", i),
        );
    }

    Ok(format!("{}{}", transformed, SHADERTOY_SUFFIX))
}

/// Detect potential parameters in Shadertoy code
pub fn detect_shadertoy_parameters(code: &str) -> Result<Vec<IsfInput>, ShadertoyConversionError> {
    let mut params = Vec::new();

    // Look for commonly tweaked variables in Shadertoy shaders
    // This is a simplified heuristic and may need improvement for real-world use

    // Check for common variables that might be parameters
    let common_params = [
        ("speed", 1.0, 0.1, 5.0),
        ("scale", 1.0, 0.1, 10.0),
        ("intensity", 1.0, 0.0, 5.0),
        ("detail", 5.0, 1.0, 10.0),
        ("color", 1.0, 0.0, 1.0),
    ];

    for (name, default, min, max) in common_params.iter() {
        // Simple regex to find variable declarations
        let pattern = format!(r"float\s+{}\s*=\s*([0-9.]+)", name);
        let re = Regex::new(&pattern).unwrap();

        if re.is_match(code) {
            params.push(IsfInput {
                name: name.to_string(),
                r#type: "float".to_string(),
                default: *default,
                min: Some(*min),
                max: Some(*max),
            });
        }
    }

    Ok(params)
}
