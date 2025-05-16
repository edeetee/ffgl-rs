#!/bin/bash

# This script uses the shadertoy crate command to create a new isf shader in the ffgl-isf/isf-extras directory,
# eg, cargo run -p shadertoy -- -u "https://www.shadertoy.com/view/ll2SRy" -o ffgl-isf/isf-extras/transparent-cube.fs
# then runs that file in Resolume using ./ffgl_run_resolume.sh.

# Usage: ./shadertoy_run_resolume.sh <shadertoy_url> <shader_name>

# Error handling
## Error if shader_name is already in use
## Error if shader_name is not provided

set -e

# Check for required arguments
if [ -z "$1" ]; then
  echo "Error: ShaderToy URL is required"
  echo "Usage: ./shadertoy_run_resolume.sh <shadertoy_url> <shader_name>"
  exit 1
fi

if [ -z "$2" ]; then
  echo "Error: Shader name is required"
  echo "Usage: ./shadertoy_run_resolume.sh <shadertoy_url> <shader_name>"
  exit 1
fi

SHADERTOY_URL="$1"
SHADER_NAME="$2"

# Ensure shader name has .fs extension
if [[ "${SHADER_NAME}" != *".fs" ]]; then
  SHADER_NAME="${SHADER_NAME}.fs"
fi

# Path to isf-extras directory
ISF_EXTRAS_DIR="$(dirname "$(realpath "$0")")/ffgl-isf/isf-extras"
OUTPUT_PATH="${ISF_EXTRAS_DIR}/${SHADER_NAME}"

# Check if shader with the same name already exists
if [ -f "$OUTPUT_PATH" ]; then
  echo "Error: Shader with name '${SHADER_NAME}' already exists in ${ISF_EXTRAS_DIR}"
  echo "Please choose a different shader name"
  exit 1
fi

echo "Converting ShaderToy shader from $SHADERTOY_URL to ISF format..."
# Run the shadertoy converter to download and convert the shader
cargo run -p shadertoy -- -u "$SHADERTOY_URL" -o "$OUTPUT_PATH"

# Check if the conversion was successful
if [ ! -f "$OUTPUT_PATH" ]; then
  echo "Error: Failed to convert ShaderToy shader"
  exit 1
fi

echo "Successfully created ISF shader: $OUTPUT_PATH"
echo "Running shader in Resolume..."

# Run the converted shader in Resolume
./ffgl_run_resolume.sh "$SHADER_NAME"

# Provide a helpful message after execution
echo "Done! Your shader '$SHADER_NAME' has been added to the ffgl-isf/isf-extras directory"
echo "You can run it again anytime with: ./ffgl_run_resolume.sh $SHADER_NAME"