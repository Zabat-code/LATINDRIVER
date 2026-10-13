#!/usr/bin/env python3
# Recorta/redimensiona las imagenes del usuario a sprites para LATINDRIVER.
from PIL import Image
import os

from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[2]
SRC = PROJECT_ROOT / "assets" / "sprites"
OUT = SRC

def save(im, name):
    dst = os.path.join(OUT, name)
    im.save(dst, "PNG")
    print(f"  -> {name} {im.size}")

# JUGADOR: 313224.jpg -> 64x64. La nina esta a la izquierda en primer plano.
im = Image.open(os.path.join(SRC, "313224.jpg")).convert("RGBA")
w, h = im.size
# recorte regional: izquierda 0-40% ancho, alto 15-85% (donde esta la nina)
box = (int(w*0.0), int(h*0.15), int(w*0.40), int(h*0.85))
crop = im.crop(box)
# recortar al centro del bounding para cuadrado
cw, ch = crop.size
side = min(cw, ch)
left = (cw - side)//2; top = (ch - side)//2
crop = crop.crop((left, top, left+side, top+side))
save(crop.resize((64, 64), Image.LANCZOS), "player.png")

# FONDO: 602575.png -> 1280x720 (cubre viewport)
im = Image.open(os.path.join(SRC, "602575.png")).convert("RGBA")
save(im.resize((1280, 720), Image.LANCZOS), "bg.png")

# OBSTACULO: 114869 (1).jpg -> 32x48 (proporcion al jugador 64x64 ~ 2:3)
im = Image.open(os.path.join(SRC, "114869 (1).jpg")).convert("RGBA")
w, h = im.size
# recorte central cuadrado luego resize a 32x48
side = min(w, h)
left = (w - side)//2; top = (h - side)//2
crop = im.crop((left, top, left+side, top+side))
save(crop.resize((32, 48), Image.LANCZOS), "obstacle.png")

print("DONE")
