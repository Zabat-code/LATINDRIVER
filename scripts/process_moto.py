#!/usr/bin/env python3
# process_moto.py - Recorta la moto generada, hace fondo transparente (chromakey
# heuristico) y reescala a 96x96 con NEAREST (rejilla del IMAGE_PROMPTS).
import os
from PIL import Image

from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
SRC = PROJECT_ROOT / "assets" / "sprites" / "generated" / "moto.png"
OUT = PROJECT_ROOT / "assets" / "sprites" / "moto_player.png"
im = Image.open(SRC).convert("RGBA")
W, H = im.size

# Color de fondo = promedio de los 4 bordes.
edges = []
for x in range(0, W, max(1, W // 20)):
    edges.append(im.getpixel((x, 0))[:3]); edges.append(im.getpixel((x, H - 1))[:3])
for y in range(0, H, max(1, H // 20)):
    edges.append(im.getpixel((0, y))[:3]); edges.append(im.getpixel((W - 1, y))[:3])
bg = [sum(c[i] for c in edges) // len(edges) for i in range(3)]

TH = 70  # umbral de distancia de color para considerar "fondo"
px = im.load()
for y in range(H):
    for x in range(W):
        r, g, b, a = px[x, y]
        d = abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2])
        if d < TH:
            px[x, y] = (r, g, b, 0)

# Recortar a la caja del contenido no transparente y reescalar a 96x96 NEAREST.
bbox = im.getbbox()
if bbox:
    im = im.crop(bbox)
im = im.resize((96, 96), Image.NEAREST)
im.save(OUT)
print("saved", OUT, im.size, im.mode)
