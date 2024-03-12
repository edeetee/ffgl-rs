#!/bin/bash
set -e

ISF_LIB_FILES=(
    "Channel Slide"
    "Dither-Bayer"
    "Radial Gradient"
    "Truchet Tile"
    # "v002-CRT-Mask" Haven't implemented the IMPORTED isf spec yet
)

ISF_EXTRA_FILES=(
    "life"
    )

deploy() {
    $(dirname $0)/deploy_isf.sh "$1"
}

for ISF_FILE in "${ISF_EXTRA_FILES[@]}"
do
    echo "Deploying $ISF_FILE"
    deploy "$(pwd $0)/example-isf/isf-extras/$ISF_FILE.fs"
done

for ISF_FILE in "${ISF_LIB_FILES[@]}"
do
    echo "Deploying $ISF_FILE"
    deploy "/Library/Graphics/ISF/$ISF_FILE.fs"
done