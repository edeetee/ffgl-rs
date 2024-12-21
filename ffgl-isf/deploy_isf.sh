#!/bin/bash
set -e

DEBUG="${DEBUG:-0}"

if [ "$DEBUG" -eq 1 ]; then
    RELEASE_TEXT=""
    PROFILE="debug"
else
    RELEASE_TEXT="--release"
fi

PREFIX="$2"

abspath() {                                               
    cd "$(dirname "$1")"
    printf "%s/%s\n" "$(pwd)" "$(basename "$1")"
}

export ISF_SOURCE="$(abspath "$1")"
FILENAME="$(basename "$ISF_SOURCE" .fs)"
export ISF_NAME="$PREFIX$(echo "$FILENAME" | cut -c1-16)"

echo "NAME: $ISF_NAME, FILE: $ISF_SOURCE"

echo "BUILDING"

cargo build $RELEASE_TEXT -p ffgl-isf

PROFILE="$PROFILE" ./deploy_bundle.sh ffgl_isf "$ISF_NAME"