#!/bin/bash
set -e

export ISF_SOURCE=$1
FILENAME=$(basename "$ISF_SOURCE" .fs)
export ISF_NAME=$(echo "$FILENAME" | cut -c1-16)

echo "NAME: $ISF_NAME, FILE: $ISF_SOURCE"

echo "BUILDING"

cargo build --release -p example-isf

./run.sh example_isf "$ISF_NAME"