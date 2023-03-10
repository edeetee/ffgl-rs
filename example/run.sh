#!/bin/bash

set -e
trap cleanup EXIT

function cleanup {
  pkill -x Arena
  cd $OLDDIR
}

OLDDIR="$(pwd)"

cd "$(dirname "$0")"

echo "Building"
cargo build --release --lib

echo "Copying to plugin bundle"
cp "target/release/libffgl_example.dylib" "/Library/Graphics/FreeFrame Plug-Ins/FFGLRsTest.bundle/Contents/MacOS/FFGLRsTest"

echo "Running resolume"
open "/Applications/Resolume Arena/Arena.app"

echo "Listening to resolume logs"
tail -F "/Users/edwardtaylor/Library/Logs/Resolume Arena/Resolume Arena log.txt"