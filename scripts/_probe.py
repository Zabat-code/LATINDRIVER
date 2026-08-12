#!/usr/bin/env python3
import urllib.request, json, urllib.parse, time, base64, os
COMFY = "http://127.0.0.1:8188"
CKPT = "v1-5-pruned.safetensors"
OUTDIR = os.path.join(os.path.dirname(__file__), "..", "assets", "sprites", "generated")
os.makedirs(OUTDIR, exist_ok=True)

def post(path, data):
    req = urllib.request.Request(COMFY + path, data=json.dumps(data).encode(),
                                 headers={"Content-Type": "application/json"})
    return json.loads(urllib.request.urlopen(req, timeout=600).read())

def get(path):
    return json.loads(urllib.request.urlopen(COMFY + path, timeout=60).read())

def build(prompt, w, h, seed, steps=20):
    return {
        "3": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
        "6": {"class_type": "CLIPTextEncode", "inputs": {"text": prompt, "clip": ["3", 1]}},
        "7": {"class_type": "CLIPTextEncode", "inputs": {"text": "blurry, low quality, deformed, photorealistic, 3d render", "clip": ["3", 1]}},
        "5": {"class_type": "EmptyLatentImage", "inputs": {"width": w, "height": h, "batch_size": 1}},
        "4": {"class_type": "KSampler", "inputs": {"seed": seed, "steps": steps, "cfg": 7.0,
                     "sampler_name": "euler", "scheduler": "normal", "denoise": 1.0,
                     "model": ["3", 0], "positive": ["6", 0], "negative": ["7", 0], "latent_image": ["5", 0]}},
        "8": {"class_type": "VAEDecode", "inputs": {"samples": ["4", 0], "vae": ["3", 2]}},
        "9": {"class_type": "SaveImage", "inputs": {"filename_prefix": "ldr", "images": ["8", 0]}},
    }

def run(name, prompt, w, h, seed):
    t0 = time.time()
    pid = post("/prompt", {"prompt": build(prompt, w, h, seed)})["prompt_id"]
    print(f"[{name}] queued {pid}")
    for _ in range(180):
        try:
            h2 = get("/history/" + pid)
            if pid in h2:
                st = h2[pid].get("status", {}).get("status_str")
                if st == "error":
                    print(f"[{name}] ERROR {str(h2[pid]['status']['messages'][-1])[:200]}")
                    return None
                for nid, o in h2[pid].get("outputs", {}).items():
                    for img in o.get("images", []):
                        v = get("/view?filename=" + urllib.parse.quote(img["filename"]) +
                                "&subfolder=" + urllib.parse.quote(img.get("subfolder", "")) +
                                "&type=" + urllib.parse.quote(img.get("type", "output")))
                        if v.get("data"):
                            outp = os.path.join(OUTDIR, name + ".png")
                            open(outp, "wb").write(base64.b64decode(v["data"][0]))
                            print(f"[{name}] SAVED {outp} in {round(time.time()-t0,1)}s")
                            return outp
                break
        except Exception:
            pass
        time.sleep(5)
    print(f"[{name}] TIMEOUT")
    return None

# Probe: primer fondo real
prompt = ("Small Caribbean village street, low colorful houses, dirt road, wooden fences, "
          "chickens and a dog on the street, tropical vegetation in background, bright midday sun, "
          "vivid colors, clear blue sky, pixel art, 2D side-scrolling game background, parallax layers, no text, no watermark")
run("bg_pueblito_dia", prompt, 768, 384, 1000)
