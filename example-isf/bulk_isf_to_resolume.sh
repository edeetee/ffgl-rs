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

abspath() {                                               
    cd "$(dirname "$1")"
    printf "%s/%s\n" "$(pwd)" "$(basename "$1")"
}

# build function
function build_isf {

    export ISF_SOURCE=$(abspath $1)
    FILENAME=$(basename "$ISF_SOURCE" .fs)
    export ISF_NAME=$(echo "$FILENAME" | cut -c1-16)

    echo "NAME: $ISF_NAME, FILE: $ISF_SOURCE"

    echo "BUILDING"

    cargo build --release -p example-isf

    ./deploy_bundle_to_resolume.sh example_isf "$ISF_NAME"
}

for ISF_FILE in "${ISF_EXTRA_FILES[@]}"
do
    build_isf "$(pwd $0)/example-isf/isf-extras/$ISF_FILE.fs"
done

for ISF_FILE in "${ISF_LIB_FILES[@]}"
do
    build_isf "/Library/Graphics/ISF/$ISF_FILE.fs"
done

./run_resolume.sh