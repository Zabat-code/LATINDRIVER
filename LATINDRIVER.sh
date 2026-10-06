#!/bin/bash
# LATINDRIVER Launcher
# Lanza el juego Godot 4.7.1 con el proyecto LATINDRIVER

cd /home/gerardo/Desktop/LATINDRIVER
export PATH="$HOME/.local/bin:$PATH"

# Si hay display disponible, lanzar directamente; si no, usar xvfb
if [ -n "$DISPLAY" ]; then
    godot --path . "$@"
else
    xvfb-run -a godot --path . "$@"
fi
