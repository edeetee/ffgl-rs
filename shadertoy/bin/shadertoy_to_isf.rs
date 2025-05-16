use anyhow::{Context, anyhow};
use clap::Parser;
use std::fs;

/// Converts Shadertoy shaders to ISF format
#[derive(Parser, Debug)]
#[command(
    version = "1.0",
    author = "ffgl-rs",
    about = "Converts Shadertoy shaders to ISF format"
)]
struct Args {
    /// URL of the Shadertoy shader
    #[arg(short, long)]
    url: String,

    /// Output file path
    #[arg(short, long)]
    output: Option<String>,

    /// Name for the shader
    #[arg(short, long)]
    name: Option<String>,

    /// API key for Shadertoy
    #[arg(short = 'k', long = "api-key", env = "SHADERTOY_API_KEY")]
    api_key: String,
}

#[tokio::main]
async fn main() {
    dotenv::dotenv().ok();

    let args = Args::parse();

    let api_key = args.api_key;
    let url = args.url;

    // Convert from URL
    println!("Fetching Shadertoy shader from URL: {}", url);
    let result = shadertoy::convert_shadertoy_url(&url, &api_key)
        .await
        .context("Failed to fetch shader");

    // Process the result
    match result {
        Ok(isf_shader) => {
            if let Some(output_path) = args.output {
                // Write to file
                if let Err(e) = fs::write(&output_path, isf_shader) {
                    eprintln!("Failed to write output file: {}", e);
                    std::process::exit(1);
                }
                println!(
                    "Shader successfully converted and saved to: {}",
                    output_path
                );
            } else {
                // Print to stdout
                println!("Converted ISF shader:");
                println!("{}", isf_shader);
            }
        }
        Err(e) => {
            eprintln!("Error: {}", e);
            std::process::exit(1);
        }
    }
}
