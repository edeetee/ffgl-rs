#!/bin/bash

ARENA="/Applications/Resolume Arena/Arena.app/Contents/MacOS/Arena"

# rm -f daw.entitlements

# codesign --display --xml --entitlements daw.entitlements "$ARENA"
# # Open daw.entitlements in a text editor and insert the required text
# sed -i '' '/<\/dict><\/plist>/i\
# <key>com.apple.security.get-task-allow<\/key><true/>\
# ' daw.entitlements

codesign -s - --deep --force --options=runtime --entitlements daw.entitlements "$ARENA"