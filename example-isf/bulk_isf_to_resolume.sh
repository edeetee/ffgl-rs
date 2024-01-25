#!/bin/bash
set -e

ISF_LIB_FILES=(
    "Channel Slide"
    "Dither-Bayer"
    "Radial Gradient"
    "Truchet Tile"
    "v002-CRT-Mask"
)

ISF_EXTRA_FILES=(
    "life"
    )

for ISF_FILE in "${ISF_FILES[@]}" "${ISF_EXTRA_FILES[@]}"
do
    export ISF_SOURCE="/Library/Graphics/ISF/$ISF_FILE.fs"
    FILENAME=$(basename "$ISF_SOURCE" .fs)
    export ISF_NAME=$(echo "$FILENAME" | cut -c1-16)

    echo "NAME: $ISF_NAME, FILE: $ISF_SOURCE"

    echo "BUILDING"

    cargo build --release -p example-isf

    ./deploy_bundle_to_resolume.sh example_isf "$ISF_NAME"
done

./run_resolume.sh