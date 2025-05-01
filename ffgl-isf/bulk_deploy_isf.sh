#!/bin/bash
set -e

# Process command line arguments
ERROR_LOGS_ONLY=false
while getopts "e" opt; do
  case $opt in
    e)
      ERROR_LOGS_ONLY=true
      ;;
    \?)
      echo "Invalid option: -$OPTARG" >&2
      exit 1
      ;;
  esac
done

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
    LOG_OUTPUT=$(CARGO_TERM_COLOR=always $(dirname $0)/deploy_isf.sh "$1" "$2" 2>&1)
    RESULT=$?
    if [ $RESULT -ne 0 ]; then
        echo ""
        echo "========================================================"
        echo "❌ ERROR deploying $1"
        echo "$LOG_OUTPUT"
        echo "========================================================"
        echo ""
        return $RESULT
    elif [ "$ERROR_LOGS_ONLY" = false ]; then
        echo "✅ Successfully deployed $1"
    fi
}

for ISF_FILE in $(pwd $0)/ffgl-isf/isf-extras/*.fs
do
    if [ "$ERROR_LOGS_ONLY" = false ]; then
        echo "Deploying $ISF_FILE"
    fi
    deploy "$ISF_FILE" || true
done

for ISF_FILE in "${ISF_LIB_FILES[@]}"
do
    if [ "$ERROR_LOGS_ONLY" = false ]; then
        echo "Deploying $ISF_FILE"
    fi
    deploy "/Library/Graphics/ISF/$ISF_FILE.fs" "v " || true
done