#!/usr/bin/env bash
# Build Primal, copy the mod folder into the Rain World install, and launch the game.
#
# Meant to run in Git Bash on the user's Windows machine (see "Deployment layout" in
# src/Primal/AI_CONTEXT.md) -- not in this container, which has no .NET toolchain and no
# Rain World install. Every path below can be overridden via environment variable so this
# still works if any of them differ on the machine it actually runs on.
#
# Usage:
#   ./scripts/build-deploy-run.sh              build, deploy, launch
#   ./scripts/build-deploy-run.sh --no-build    skip dotnet build, just redeploy + launch
#   ./scripts/build-deploy-run.sh --no-launch   build + deploy only, don't start the game
set -euo pipefail

SRC_DIR="${PRIMAL_SRC_DIR:-/c/Users/rokas/Documents/RWMods/src/Primal}"
GAME_DIR="${RAINWORLD_GAME_DIR:-/c/Program Files (x86)/Steam/steamapps/common/Rain World}"
MOD_DEST="${RAINWORLD_MOD_DEST:-$GAME_DIR/RainWorld_Data/StreamingAssets/mods/Primal}"

DO_BUILD=1
DO_LAUNCH=1
for arg in "$@"; do
    case "$arg" in
        --no-build) DO_BUILD=0 ;;
        --no-launch) DO_LAUNCH=0 ;;
        *) echo "Unknown option: $arg" >&2; exit 1 ;;
    esac
done

if [[ "$DO_BUILD" -eq 1 ]]; then
    echo "==> Building Primal ($SRC_DIR)"
    cd "$SRC_DIR"
    dotnet build
    # Primal.csproj's own CopyToModFolder target already drops the freshly-built DLL into
    # mod/plugins/ after a successful build, so mod/ is ready to copy as-is below.
else
    echo "==> Skipping build (--no-build)"
    cd "$SRC_DIR"
fi

echo "==> Deploying mod/ to $MOD_DEST"
mkdir -p "$MOD_DEST/plugins"
cp -f "mod/modinfo.json" "$MOD_DEST/modinfo.json"
cp -f "mod/plugins/Primal.dll" "$MOD_DEST/plugins/Primal.dll"

if [[ "$DO_LAUNCH" -eq 1 ]]; then
    echo "==> Launching Rain World ($GAME_DIR)"
    cd "$GAME_DIR"
    ./RainWorld.exe &
    disown
else
    echo "==> Skipping launch (--no-launch)"
fi

echo "==> Done."
