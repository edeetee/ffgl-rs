#!/bin/sh

set -e

PROFILE="${PROFILE:-release}"

LIB_NAME=$1
BUNDLE_NAME=${2:-$LIB_NAME}

# Detect platform and set up directories
case "$(uname -s)" in
    Darwin*)
        PLATFORM="macos"
        LIB_PATH="target/$PROFILE/lib$LIB_NAME.dylib"
        FFGL_DIRS=(
            "$HOME/Library/Graphics/FreeFrame Plug-Ins"
            "$HOME/Documents/Resolume Arena/Extra Effects"
        )
        ;;
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        PLATFORM="windows"
        LIB_PATH="target/$PROFILE/$LIB_NAME.dll"
        FFGL_DIRS=(
            "$USERPROFILE/Documents/Resolume Arena/Extra Effects"
            "$APPDATA/Resolume Arena/Extra Effects"
        )
        ;;
    *)
        echo "Unsupported platform: $(uname -s)"
        exit 1
        ;;
esac

if [ ! -f "$LIB_PATH" ]; then
    echo "$LIB_PATH not found!"
    exit 1
fi

for FFGL_DIR in "${FFGL_DIRS[@]}"; do
    if [ "$PLATFORM" = "macos" ]; then
        # macOS: Create bundle
        OUT_BUNDLE_DIR="$FFGL_DIR/$BUNDLE_NAME.bundle"
        echo "Creating bundle in $OUT_BUNDLE_DIR"
        mkdir -p "$OUT_BUNDLE_DIR/Contents/MacOS"
        cp "$LIB_PATH" "$OUT_BUNDLE_DIR/Contents/MacOS/$BUNDLE_NAME"
        echo "copying $LIB_PATH as $BUNDLE_NAME into $OUT_BUNDLE_DIR"
    else
        # Windows: Copy DLL directly
        if [ -d "$FFGL_DIR" ]; then
            echo "Copying DLL to $FFGL_DIR"
            cp "$LIB_PATH" "$FFGL_DIR/$BUNDLE_NAME.dll"
            echo "copying $LIB_PATH as $BUNDLE_NAME.dll into $FFGL_DIR"
        else
            echo "Directory not found: $FFGL_DIR (skipping)"
        fi
    fi
done
