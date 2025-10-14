#!/bin/bash

set -e

vsVersion="1.21.5"
vsImGuiVersion="1.1.14"
configLibVersion="1.10.6"
autoConfigLibVersion="2.0.9"

# Create directories
directories=(
    "DurableBetterProspecting/run/Mods"
    "vendor"
)
for directory in "${directories[@]}"; do
    if [[ ! -d "$directory" ]]; then
        mkdir -p "$directory"
    fi
done

# Fetch Vintage Story files
vsFiles=(
    "Lib/protobuf-net.dll"
    "Lib/SkiaSharp.dll"
    "Mods/VSEssentials.dll"
    "Mods/VSSurvivalMod.dll"
    "VintagestoryAPI.dll"
    "VintagestoryLib.dll"
)
for vsFile in "${vsFiles[@]}"; do
    baseVsFile=$(basename "$vsFile")
    if [[ ! -f "vendor/$baseVsFile" ]]; then
        if [[ "$1" == "remote" ]]; then
            curl -L "https://files.omni.ms/protected/woodpecker/vintagestory/${vsVersion}/${vsFile}?raw" -o "vendor/${baseVsFile}" -u "${REMOTE_USERNAME}:${REMOTE_PASSWORD}"
        else
            cp "${VINTAGE_STORY}/${vsFile}" "vendor/${baseVsFile}"
        fi
    fi
done

# 3rd party mods
vsImGuiName="58527/vsimgui_${vsImGuiVersion}.zip"
vsImGuiFiles=("ImGui.NET.dll" "VSImGui.dll")

configLibName="57734/configlib_${configLibVersion}.zip"
configLibFiles=("configlib.dll")

autoConfigLibName="54531/autoconfiglib_${autoConfigLibVersion}.zip"
autoConfigLibFiles=()

mods=(
    "vsImGui"
    "configLib"
    "autoConfigLib"
)

for mod in "${mods[@]}"; do
    declare -n modName="${mod}Name"
    declare -n modFiles="${mod}Files"
    baseMod=$(basename "$modName")
    if [[ ! -f "vendor/${baseMod}" ]]; then
        # Download mod
        curl -L "https://mods.vintagestory.at/download/${modName}" -o "vendor/${baseMod}"
        cp "vendor/${baseMod}" "DurableBetterProspecting/run/Mods/${baseMod}"

        # Extract to temporary path
        tempDir=$(mktemp -d)
        unzip "vendor/${baseMod}" -d "$tempDir"

        # Copy contents
        for file in "${modFiles[@]}"; do
            cp "${tempDir}/${file}" "vendor/${file}"
        done

        # Clean up
        rm -rf "$tempDir"
    fi
done
