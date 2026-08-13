#!/usr/bin/env python3
"""Pipeline de generacion de imagenes para LATINDRIVER (OFFLINE-SAFE).

Similar a Braviestale/tools/generate_images.py pero para los assets de LATINDRIVER
(fondos de avenida/ciudad colonial dia/tarde/noche y sprites de moto). Requiere
un motor de imagen (GPU local ComfyUI/SD, o API con KEY). Sin eso, modo 'plan'
solo escribe assets_plan.json.

Uso:
  python tools/gen_images_ld.py --engine openai --variants 2 --limit 40
  (sin KEY/GPU cae a modo plan: deja assets_plan.json)
"""
import os, json, argparse

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUTDIR = os.path.join(BASE, "assets", "sprites", "generated")

# Prompts base por defecto (estilo "Concert Hall" / caricatura latina, coherente
# con design.md de LATINDRIVER). Ampliable: anyade entradas al dict PROMPTS.
PROMPTS = [
    ("avenida_dia", "background", "2D side-scrolling endless-runner background, avenida colonial latinoamericana de dia, edificios de colores, faroles, banquetas, cielo despejado, estilo cartoon limpio coherente con 'Concert Hall', tileable horizontal, no personajes, 16:9"),
    ("avenida_tarde", "background", "2D side-scroller background, avenida colonial al atardecer, luz naranja calida, siluetas de edificios, estilo cartoon, tileable horizontal, 16:9"),
    ("avenida_noche", "background", "2D side-scroller background, avenida colonial de noche, faroles encendidos, cielo purpura, estilo cartoon, tileable horizontal, 16:9"),
    ("ciudad_dia", "background", "2D side-scroller background, centro colonial latinoamericano de dia, plaza, fuente, estilo cartoon, tileable horizontal, 16:9"),
    ("ciudad_tarde", "background", "2D side-scroller background, centro colonial al atardecer, estilo cartoon, tileable horizontal, 16:9"),
    ("ciudad_noche", "background", "2D side-scroller background, centro colonial de noche, luces de ventanas, estilo cartoon, tileable horizontal, 16:9"),
    ("moto_azul", "character", "64x64 pixel-art cartoon motorcycle, blue, side view, two wheels, handlebar, headlight, clean shape, transparent background"),
    ("moto_roja", "character", "64x64 pixel-art cartoon motorcycle, red, side view, two wheels, handlebar, headlight, clean shape, transparent background"),
    ("moto_verde", "character", "64x64 pixel-art cartoon motorcycle, green, side view, two wheels, handlebar, headlight, clean shape, transparent background"),
    ("gameover", "ui", "cartoon accident crash illustration, tipped motorcycle, comic 'POW' style, big 'GAME OVER' text, funny not gore, 16:9"),
]

def build_plan(limit, variants):
    plan = []
    count = 0
    for tid, kind, prompt in PROMPTS:
        for v in range(variants):
            if count >= limit:
                break
            name = tid + (f"_{v+1}" if variants > 1 else "")
            plan.append({"out": os.path.join(OUTDIR, name + ".png"), "prompt": prompt, "kind": kind, "source_id": tid, "variant": v + 1})
            count += 1
        if count >= limit:
            break
    os.makedirs(OUTDIR, exist_ok=True)
    with open(os.path.join(BASE, "assets_plan.json"), "w", encoding="utf-8") as f:
        json.dump({"count": len(plan), "items": plan}, f, ensure_ascii=False, indent=2)
    return plan

def generate(plan, engine):
    if engine == "none":
        print(f"[PLAN] {len(plan)} imagenes planeadas (motor 'none', no se generan)")
        return
    if engine == "openai":
        from openai import OpenAI
        client = OpenAI()
        for it in plan:
            r = client.images.generate(model="gpt-image-1", prompt=it["prompt"], size="1024x1024", n=1)
            print("  gen", it["out"])
    else:
        print(f"[WARN] motor {engine} no implementado en stub")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--engine", default="none", choices=["none", "openai", "replicate", "comfyui"])
    ap.add_argument("--variants", type=int, default=2)
    ap.add_argument("--limit", type=int, default=40)
    args = ap.parse_args()
    plan = build_plan(args.limit, args.variants)
    print(f"Plan LATINDRIVER: {len(plan)} imagenes (engine={args.engine})")
    generate(plan, args.engine)

if __name__ == "__main__":
    main()
