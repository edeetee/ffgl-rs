#!/bin/bash

set -e
trap cleanup EXIT

function cleanup {
  pkill -x Arena
  cd $OLDDIR
}

LIB_NAME=$1
BUNDLE_NAME=${2:-$LIB_NAME}

OLDDIR="$(pwd)"

cd "$(dirname "$0")"

LIB_PATH="target/release/lib$LIB_NAME.dylib"

FFGL_DIR="/Library/Graphics/FreeFrame Plug-Ins"
OUT_BUNDLE_DIR="$FFGL_DIR/$BUNDLE_NAME.bundle"

echo "Creating bundle in $OUT_BUNDLE_DIR"

mkdir -p "$OUT_BUNDLE_DIR/Contents/MacOS"
cp "$LIB_PATH" "$OUT_BUNDLE_DIR/Contents/MacOS/$BUNDLE_NAME"

echo "copying $LIB_PATH as $BUNDLE_NAME into $OUT_BUNDLE_DIR"


echo "Running resolume"
open "/Applications/Resolume Arena/Arena.app"

echo "Listening to resolume logs"
tail -F "/Users/edwardtaylor/Library/Logs/Resolume Arena/Resolume Arena log.txt"