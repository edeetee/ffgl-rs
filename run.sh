#!/bin/bash

set -e
trap cleanup EXIT

function cleanup {
  pkill -x Arena
  cd $OLDDIR
}

LIB_NAME=$1

OLDDIR="$(pwd)"

cd "$(dirname "$0")"

LIB_PATH="target/release/lib$LIB_NAME.dylib"

FFGL_DIR="/Library/Graphics/FreeFrame Plug-Ins"
OUT_BUNDLE_DIR="$FFGL_DIR/$LIB_NAME.bundle"

echo "Creating bundle in $OUT_BUNDLE_DIR"

mkdir -p "$OUT_BUNDLE_DIR/Contents/MacOS"
cp "$LIB_PATH" "$OUT_BUNDLE_DIR/Contents/MacOS/$LIB_NAME"

echo "copying $LIB_PATH into $OUT_BUNDLE_DIR"


echo "Running resolume"
open "/Applications/Resolume Arena/Arena.app"

echo "Listening to resolume logs"
tail -F "/Users/edwardtaylor/Library/Logs/Resolume Arena/Resolume Arena log.txt"