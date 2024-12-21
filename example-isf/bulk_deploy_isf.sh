#!/bin/bash
set -e

ISF_LIB_FILES=(
    "Channel Slide"
    "Dither-Bayer"
    "Radial Gradient"
    "Truchet Tile"
    "CMYK Halftone-Lookaround"
    "CMYK Halftone"
    # "Noise" Customised
    # "v002-CRT-Mask" Haven't implemented the IMPORTED isf spec yet
)

deploy() {
    $(dirname $0)/deploy_isf.sh "$1"
}

for ISF_FILE in $(pwd $0)/example-isf/isf-extras/*.fs
do
    echo "Deploying $ISF_FILE"
    deploy "$ISF_FILE" || echo "Failed to deploy $ISF_FILE" || true
done

for ISF_FILE in "${ISF_LIB_FILES[@]}"
do
    echo "Deploying $ISF_FILE"
    deploy "/Library/Graphics/ISF/$ISF_FILE.fs" "vdmx_"
done