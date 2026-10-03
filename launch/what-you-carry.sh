#!/usr/bin/env bash
# Update, build, and start What You Carry on the Steam Deck, or on another Linux machine (README, "Launch the game").
#
#   what-you-carry              update the checkout to the newest main, build, and start the game
#   what-you-carry --no-update  build and start the checkout as it is
#   what-you-carry --install    add the command what-you-carry and the desktop shortcut, and start nothing
#
# The update stops when a tracked file has a change, so it never discards work. The environment variable WYC_GODOT
# names the Godot .NET binary, as for the test suite. Without it, the script uses the binary of the Deck (D-294, D-428).
# When a step fails, the terminal stays open until Enter, so a desktop shortcut shows the error.
set -euo pipefail

GODOT_VERSION="4.7.2-stable"
DEFAULT_GODOT="$HOME/godot/Godot_v${GODOT_VERSION}_mono_linux_x86_64/Godot_v${GODOT_VERSION}_mono_linux.x86_64"
SCRIPT="$(readlink -f "${BASH_SOURCE[0]}")"
REPO="$(dirname "$(dirname "$SCRIPT")")"
GAME_DIR="$REPO/WhatYouCarry.Game"
BIN_DIR="$HOME/.local/bin"
SHORTCUT="$HOME/Desktop/what-you-carry.desktop"

STEP="arguments"
# Ends the script with the code. In a terminal, it waits for Enter first, so the window of a shortcut stays open.
stop() {
    if [ -t 0 ]; then
        read -r -p "Push Enter to close." _ || true
    fi
    exit "$1"
}

on_error() {
    local code=$?
    echo "what-you-carry: the step '$STEP' failed with exit code $code." >&2
    stop "$code"
}
trap on_error ERR

fail() {
    echo "what-you-carry: the step '$STEP' failed: $1" >&2
    stop 1
}

install() {
    STEP="install"
    mkdir -p "$BIN_DIR" "$(dirname "$SHORTCUT")"
    ln -sfn "$SCRIPT" "$BIN_DIR/what-you-carry"
    cat > "$SHORTCUT" <<EOF
[Desktop Entry]
Type=Application
Name=What You Carry
Comment=Update to the newest main, build, and start the game
Exec=$BIN_DIR/what-you-carry
Path=$REPO
Icon=applications-games
Terminal=true
StartupNotify=true
EOF
    chmod +x "$SHORTCUT"
    echo "what-you-carry: the command is $BIN_DIR/what-you-carry, and the shortcut is $SHORTCUT."
}

update() {
    STEP="update"
    echo "== update: the newest main"
    git -C "$REPO" fetch origin main
    if [ -n "$(git -C "$REPO" status --porcelain --untracked-files=no)" ]; then
        fail "a tracked file in $REPO has a change. Commit or remove the change, or start with --no-update."
    fi
    git -C "$REPO" checkout main
    git -C "$REPO" pull --ff-only origin main
    git -C "$REPO" log -1 --oneline
}

update_checkout=1
case "${1:-}" in
    "") ;;
    --no-update) update_checkout=0 ;;
    --install) install; exit 0 ;;
    *) fail "unknown argument '$1'. Use --no-update, --install, or no argument." ;;
esac

if [ -d "$HOME/.dotnet" ]; then
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$HOME/.dotnet:$PATH"
fi
GODOT="${WYC_GODOT:-$DEFAULT_GODOT}"
STEP="check"
if [ ! -x "$GODOT" ]; then
    fail "no Godot .NET binary at $GODOT. Unzip Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip into $HOME/godot, or set WYC_GODOT."
fi
command -v dotnet > /dev/null || fail "no dotnet on the command path. Install the SDK version of $REPO/global.json in $HOME/.dotnet."

if [ "$update_checkout" = 1 ]; then
    update
fi

STEP="build"
echo "== build: dotnet build"
dotnet build "$REPO/WhatYouCarry.slnx"

STEP="import"
echo "== import: the Godot import and the C# build"
"$GODOT" --headless --editor --path "$GAME_DIR" --build-solutions --quit

STEP="play"
echo "== play: Escape or Start ends the session (D-311)"
"$GODOT" --path "$GAME_DIR"
