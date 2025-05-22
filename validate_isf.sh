#!/bin/bash

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <path_to_isf_shader>"
    exit 1
fi

# Run the validator
cargo run -p build-common --bin isf_validator "$1"