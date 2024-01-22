#!/bin/sh

set -e
trap cleanup EXIT

function cleanup {
  pkill -9 -x Arena
}

echo "Running resolume"
/Applications/Resolume\ Arena/Arena.app/Contents/MacOS/Arena &

sleep 0.5

echo "Listening to resolume logs"
tail -n 0 -F "$HOME/Library/Logs/Resolume Arena/Resolume Arena log.txt"