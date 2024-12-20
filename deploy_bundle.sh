#!/bin/sh

set -e

PROFILE="${PROFILE:-release}"

LIB_NAME=$1
BUNDLE_NAME=${2:-$LIB_NAME}

LIB_PATH="target/$PROFILE/lib$LIB_NAME.dylib"

if [ ! -f "$LIB_PATH" ]; then
    echo "$LIB_PATH not found!"
    exit 1
fi

FFGL_COMMON_DIR="$HOME/Library/Graphics/FreeFrame Plug-Ins"
FFGL_RESOLUME_DIR="$HOME/Documents/Resolume Arena/Extra Effects"

for FFGL_DIR in "$FFGL_COMMON_DIR" "$FFGL_RESOLUME_DIR"; do
    OUT_BUNDLE_DIR="$FFGL_DIR/$BUNDLE_NAME.bundle"

    echo "Creating bundle in $OUT_BUNDLE_DIR"

    mkdir -p "$OUT_BUNDLE_DIR/Contents/MacOS"
    cp "$LIB_PATH" "$OUT_BUNDLE_DIR/Contents/MacOS/$BUNDLE_NAME"

    echo "copying $LIB_PATH as $BUNDLE_NAME into $OUT_BUNDLE_DIR"
done
