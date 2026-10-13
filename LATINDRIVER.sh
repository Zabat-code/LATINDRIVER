#!/bin/bash
# LATINDRIVER Launcher
# Lanza el juego Godot 4.7.1 con el proyecto LATINDRIVER

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"
export PATH="$HOME/.local/bin:$PATH"

# Si hay display disponible, lanzar directamente; si no, usar xvfb
if [ -n "$DISPLAY" ]; then
    godot --path . "$@"
else
    xvfb-run -a godot --path . "$@"
fi
