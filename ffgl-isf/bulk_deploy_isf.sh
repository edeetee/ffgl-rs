#!/bin/bash
set -e



ISF_LIB_FILES=(
    "Channel Slide"
    "Dither-Bayer"
    "Truchet Tile"
    # "CMYK Halftone-Lookaround"
    "CMYK Halftone"
    "Sorting Smear"
    "Thermal Camera"
    "Random Freeze"
    "Multi-Pixellate"
    "Dot Screen"
    # "Noise" Customised
    # "v002-CRT-Mask" Haven't implemented the IMPORTED isf spec yet
)

deploy() {
    LOG_OUTPUT=$($(dirname $0)/deploy_isf.sh "$1" "$2" 2>&1)
    RESULT=$?
    if [ $RESULT -ne 0 ]; then
        if [ ! -z "$DEBUG" ]; then
            echo "$LOG_OUTPUT"
        fi
    fi
    return $RESULT
}

for ISF_FILE in $(pwd $0)/ffgl-isf/isf-extras/*.fs
do
    echo "Deploying $ISF_FILE"
    deploy "$ISF_FILE" || echo "ERROR deploying $ISF_FILE" || true
done

for ISF_FILE in "${ISF_LIB_FILES[@]}"
do
    echo "Deploying $ISF_FILE"
    deploy "/Library/Graphics/ISF/$ISF_FILE.fs" "v "
done