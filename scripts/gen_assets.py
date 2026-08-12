#!/usr/bin/env python3
# gen_assets.py - Genera el set de assets de LATINDRIVER via ComfyUI API (SD1.5 fp16, CPU).
# Basado en IMAGE_PROMPTS.txt: 15 fondos (5 mapas x 3 turnos) + moto principal.
# El server guarda en F:/ComfyUI/output/ con prefix unico por job; este cliente
# hace poll del filesystem (no depende de /history ni /view, que fallaban).
import json, urllib.request, urllib.parse, time, os, sys, glob

COMFY = "http://127.0.0.1:8188"
CKPT = "v1-5-pruned-fp16.safetensors"
OUTDIR = os.path.join(os.path.dirname(__file__), "..", "assets", "sprites", "generated")
COMFY_OUT = r"F:\ComfyUI\output"
os.makedirs(OUTDIR, exist_ok=True)

BG_SUFFIX = "pixel art, 2D side-scrolling game background, parallax layers, no text, no watermark"

MAPS = {
    "pueblito": "Small Caribbean village street, low colorful houses, dirt road, wooden fences, chickens and a dog on the street, tropical vegetation in background",
    "ciudad_colonial": "Colonial Caribbean city street, cobblestone road, golden limestone building facades, wrought iron balconies, old street lamps, small cars and delivery vans, warm golden hour light",
    "megaciudad": "Modern dense Caribbean city avenue, multi-story concrete buildings, wide multi-lane road, SUVs and long trucks in traffic, urban skyline, late afternoon light",
    "ciudad_marginada": "Poor urban neighborhood street, cracked asphalt with potholes, low hanging power cables, makeshift houses, scattered trash, stray dogs on the street, overcast light",
    "avenida": "Open rural highway between provinces, two-lane road, tropical countryside, fast cars and cargo trucks, palm trees and hills in background, warm late afternoon light",
}
TURNS = {
    "dia": "bright midday sun, vivid colors, clear blue sky",
    "tarde": "warm afternoon light",
    "noche": "dark blue night sky, few warm streetlights, dim lighting, glowing windows, headlights",
}
MOTO = ("Pixel art motorcycle side view, rider with helmet, Caribbean delivery style, "
        "simple clean sprite, transparent background, game character sprite sheet style, no text")

JOBS = []
for mname, mbase in MAPS.items():
    for tname, tlight in TURNS.items():
        prompt = f"{mbase}, {tlight}, {BG_SUFFIX}"
        JOBS.append((f"bg_{mname}_{tname}", prompt, 640, 320))
JOBS.append(("moto", MOTO, 512, 512))


def queue(prompt, prefix, w, h, seed, steps=15):
    p = {
        "3": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": prompt, "clip": ["3", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": "blurry, low quality, deformed, photorealistic, 3d render", "clip": ["3", 1]}},
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": w, "height": h, "batch_size": 1}},
        "4": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": 7.0,
                     "sampler_name": "euler", "scheduler": "normal", "denoise": 1.0,
                     "model": ["3", 0], "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["5", 0]}},
        "8": {"class_type": "VAEDecode", "inputs": {"samples": ["4", 0], "vae": ["3", 2]}},
        "9": {"class_type": "SaveImage", "inputs": {"filename_prefix": prefix, "images": ["8", 0]}},
    }
    return json.loads(urllib.request.urlopen(urllib.request.Request(
        COMFY + "/prompt", data=json.dumps({"prompt": p}).encode(),
        headers={"Content-Type": "application/json"}), timeout=120).read())["prompt_id"]


def wait_file(prefix, timeout=900):
    # El server guarda F:/ComfyUI/output/{prefix}_00001_.png (o _00002 si repite).
    deadline = time.time() + timeout
    while time.time() < deadline:
        matches = glob.glob(os.path.join(COMFY_OUT, prefix + "_*.png"))
        if matches:
            return max(matches, key=os.path.getmtime)
        time.sleep(5)
    return None


def main():
    only = set(sys.argv[1:])
    for i, (name, prompt, w, h) in enumerate(JOBS):
        if only and name not in only:
            continue
        dest = os.path.join(OUTDIR, name + ".png")
        if os.path.exists(dest):
            print(f"[{name}] skip (ya existe)")
            continue
        seed = 1000 + i * 7
        prefix = "ldr_" + name  # prefix unico para mapear al archivo
        print(f"[{name}] queue seed={seed} prefix={prefix}")
        queue(prompt, prefix, w, h, seed)
        f = wait_file(prefix)
        if f:
            import shutil
            shutil.copyfile(f, dest)
            print(f"[{name}] SAVED -> {dest} ({os.path.getsize(dest)} bytes)")
        else:
            print(f"[{name}] TIMEOUT esperando archivo")


if __name__ == "__main__":
    main()
