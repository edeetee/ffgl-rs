#!/bin/bash

set -e

: ${LEVEL:=debug}
: ${FIELD_X:=}

# If $1 is empty, run bulk_deploy_isf.sh intead
if [ -z "$1" ]; then
    export RUST_LOG=$LEVEL
    ./ffgl-isf/bulk_deploy_isf.sh
    ./resolume.sh
    exit 0
fi

# $1 may exclude the .fs extension
# $1 may be a full path or just the filename
# $1 may be a relative path to either the current directory or the ISF directory
ISF_FILE_IN=$1

FFGL_EXTRA_PATH=$(dirname "$(realpath "$0")")/ffgl-isf/isf-extras
FFGL_LIB_PATH=/Library/Graphics/ISF

if [[ "$ISF_FILE_IN" != /* ]]; then
    if [[ "${ISF_FILE_IN##*.}" != "fs" ]]; then
        ISF_FILE_IN="$ISF_FILE_IN.fs"
    fi
    if [[ -f "$ISF_FILE_IN" ]]; then
        ISF_FILE=$(realpath "$ISF_FILE_IN")
    elif [[ -f "$FFGL_EXTRA_PATH/$ISF_FILE_IN" ]]; then
        ISF_FILE=$(realpath "$FFGL_EXTRA_PATH/$ISF_FILE_IN")
    elif [[ -f "$FFGL_LIB_PATH/$ISF_FILE_IN" ]]; then
        ISF_FILE=$(realpath "$FFGL_LIB_PATH/$ISF_FILE_IN")
    else
        echo "File not found: $ISF_FILE_IN"
        exit 1
    fi
else
    ISF_FILE="$ISF_FILE_IN"
    if [[ "${ISF_FILE##*.}" != "fs" ]]; then
        ISF_FILE="$ISF_FILE.fs"
    fi
fi

FILENAME="$(basename "$ISF_FILE" .fs)"
export ISF_NAME="$PREFIX$(echo "$FILENAME" | cut -c1-16)"

# DEBUG=1
export RUST_LOG="[entry{name=.*$ISF_NAME.*}$FIELD_X]=$LEVEL"

ffgl-isf/deploy_isf.sh "$ISF_FILE"
echo "RUST_LOG=$RUST_LOG"

# Check arguments for the vdmx flag
RUN_VDMX=0
for arg in "$@"; do
    if [ "$arg" == "--vdmx" ]; then
        RUN_VDMX=1
        break
    fi
done

# If vdmx flag is present, run vdmx.sh and exit
if [ "$RUN_VDMX" -eq 1 ]; then
        ./vdmx.sh
        exit 0
fi

./resolume.sh