#!/bin/bash
set -e

DEBUG="${DEBUG:-0}"

if [ "$DEBUG" -eq 1 ]; then
    RELEASE_TEXT=""
    PROFILE="debug"
else
    RELEASE_TEXT="--release"
fi

abspath() {                                               
    cd "$(dirname "$1")"
    printf "%s/%s\n" "$(pwd)" "$(basename "$1")"
}

export ISF_SOURCE="$(abspath "$1")"
FILENAME="$(basename "$ISF_SOURCE" .fs)"
export ISF_NAME="$(echo "$FILENAME" | cut -c1-16)"

echo "NAME: $ISF_NAME, FILE: $ISF_SOURCE"

echo "BUILDING"

cargo build $RELEASE_TEXT -p example-isf

PROFILE="$PROFILE" ./deploy_bundle.sh example_isf "$ISF_NAME"