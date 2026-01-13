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

ISF_DIRECTORIES=(
    "$(dirname $0)/isf-extras"
    "$(dirname $0)/projectileobjects-MiscISFShaders"
)

ISF_LIB_FILES=(
    "Channel Slide"
    "Dither-Bayer"
    "Truchet Tile"
    # "CMYK Halftone-Lookaround"
    "CMYK Halftone"
    # "Sorting Smear"
    # "Thermal Camera"
    # "Random Freeze"
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

for ISF_DIR in "${ISF_DIRECTORIES[@]}"
do
    if [ ! -d "$ISF_DIR" ]; then
        if [ "$ERROR_LOGS_ONLY" = false ]; then
            echo "⚠️ Skipping directory $ISF_DIR (not found)"
        fi
        continue
    fi

    for ISF_FILE in "$ISF_DIR"/*.fs
    do
        if [ ! -f "$ISF_FILE" ]; then
            continue
        fi
        
        if [ "$ERROR_LOGS_ONLY" = false ]; then
            echo "Deploying $ISF_FILE"
        fi
        deploy "$ISF_FILE" || true
    done
done

for ISF_FILE_NAME in "${ISF_LIB_FILES[@]}"
do
    ISF_FILE="/Library/Graphics/ISF/$ISF_FILE_NAME.fs"
    
    if [ ! -f "$ISF_FILE" ]; then
        if [ "$ERROR_LOGS_ONLY" = false ]; then
            echo "⚠️ Skipping $ISF_FILE (not found)"
        fi
        continue
    fi

    if [ "$ERROR_LOGS_ONLY" = false ]; then
        echo "Deploying $ISF_FILE"
    fi
    deploy "$ISF_FILE" "v " || true
done