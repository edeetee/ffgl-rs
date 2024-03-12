#!/bin/sh

set -e
trap cleanup EXIT

function cleanup {
  pkill -9 -x VDMX5
  echo "script killed"
}

echo "Running VDMX5"
/Applications/VDMX5.app/Contents/MacOS/VDMX5 &

sleep .5

#until there is a new file in the logs folder
# until /bin/ls "$HOME/Library/Logs/VDMX5" -lt modified > /dev/null 2>&1
# do
#   sleep 1
#   echo "waiting for log file"
# done

echo "Listening to VDMX5 logs"
log_dir="$HOME/Library/Logs/VDMX5"
log_file="$log_dir/$(/bin/ls -t $log_dir | head -1)"

echo "Log file: $log_file"
tail -f -n +1 "$log_file"