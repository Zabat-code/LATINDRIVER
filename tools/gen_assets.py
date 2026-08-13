#!/usr/bin/env python3
"""Generate LATINDRIVER placeholder art + a loopable music track (offline, no GPU).
Motorcycle sprites (64x64, 3 designs), GAME OVER cartoon, and a short loopable
OGG tune (procedural). MIDI from C: cannot be rendered offline (ffmpeg has no
MIDI decoder, no fluidsynth) -> drop converted .ogg files into assets/audio/music/.
"""
import os, struct, wave, math, subprocess, sys
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPR = os.path.join(ROOT, "assets", "sprites")
AUD = os.path.join(ROOT, "assets", "audio", "music")
os.makedirs(SPR, exist_ok=True)
os.makedirs(AUD, exist_ok=True)

# ---------- 64x64 cartoon motorcycles (3 designs) ----------
def wheel(d, cx, cy, r, col):
    d.ellipse([cx-r, cy-r, cx+r, cy+r], fill=(30,30,30), outline=(10,10,10))
    d.ellipse([cx-r//2, cy-r//2, cx+r//2, cy+r//2], fill=col, outline=(80,80,80))

def moto(idx, body, accent):
    img = Image.new("RGBA", (64,64), (0,0,0,0))
    d = ImageDraw.Draw(img)
    # wheels
    wheel(d, 16, 48, 9, (120,120,120))
    wheel(d, 48, 48, 9, (120,120,120))
    # body
    d.polygon([(12,46),(20,30),(44,30),(52,46)], fill=body, outline=(20,20,20))
    # seat/tank
    d.rectangle([26,26,42,32], fill=accent, outline=(20,20,20))
    # handlebar
    d.line([(44,30),(54,18)], fill=(40,40,40), width=3)
    d.line([(54,18),(58,18)], fill=(40,40,40), width=3)
    # headlight
    d.ellipse([(50,26),(58,34)], fill=(255,240,160), outline=(180,160,80))
    if idx == 2:  # sport fairing
        d.polygon([(40,30),(58,22),(58,30)], fill=accent, outline=(20,20,20))
    if idx == 3:  # chopper long fork
        d.line([(44,30),(60,20)], fill=(60,60,60), width=2)
        d.line([(16,46),(60,20)], fill=(70,70,70), width=2)
    img.save(os.path.join(SPR, f"moto{idx}.png"))
    print("wrote moto", idx)

moto(1, (200,40,40), (240,200,80))   # red cafe racer
moto(2, (40,90,200), (180,220,255))  # blue sport
moto(3, (40,160,70), (220,220,120))  # green chopper

# ---------- GAME OVER cartoon (480x270) ----------
gw, gh = 480, 270
go = Image.new("RGBA", (gw, gh), (0,0,0,0))
d = ImageDraw.Draw(go)
# sky
d.rectangle([0,0,gw,gh*0.55], fill=(250,180,120))
# road
d.rectangle([0,int(gh*0.55),gw,gh], fill=(90,90,95))
d.line([0,int(gh*0.55),gw,int(gh*0.55)], fill=(60,60,60), width=3)
# tipped motorcycle (cartoon crash)
d.ellipse([150,150,200,200], fill=(40,40,40))  # wheels
d.ellipse([250,150,300,200], fill=(40,40,40))
d.polygon([(150,170),(300,170),(280,140),(170,140)], fill=(200,40,40), outline=(20,20,20))
d.line([(180,140),(210,110)], fill=(40,40,40), width=4)  # broken handlebar
# motion/skid marks
for i in range(3):
    d.line([(140-i*30,210),(160-i*30,210)], fill=(40,40,40), width=4)
# GAME OVER text (big)
d.text((gw//2-150, 30), "GAME OVER", fill=(40,0,0))
go.save(os.path.join(SPR, "gameover.png"))
print("wrote gameover.png")

# ---------- procedural loopable tune -> OGG ----------
def note_freq(n): return 440.0 * (2**((n-69)/12.0))
sr = 22050
beats = [60,64,67,72,67,64,60,64]  # simple loopable arpeggio (C major)
dur = 0.28
frames = []
for b in beats:
    f = note_freq(b)
    for i in range(int(sr*dur)):
        t = i/sr
        v = math.sin(2*math.PI*f*t)*0.25 + math.sin(2*math.pi*f*2*t)*0.08
        frames.append(struct.pack('<h', int(max(-1,min(1,v))*32000)))
# second layer bass
for b in [36,36,43,43,41,41,36,36]:
    f = note_freq(b); 
    for i in range(int(sr*dur)):
        t=i/sr; v=math.sin(2*math.PI*f*t)*0.12
        frames.append(struct.pack('<h', int(max(-1,min(1,v))*32000)))
raw = b''.join(frames)
wav = os.path.join(AUD, "_loop.wav")
with wave.open(wav, 'w') as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(sr); w.writeframes(raw)
ogg = os.path.join(AUD, "loop1.ogg")
subprocess.run(["ffmpeg","-y","-i",wav,"-acodec","libvorbis",ogg],
               stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
os.remove(wav)
print("wrote loop1.ogg" if os.path.exists(ogg) else "OGG FAILED")
print("DONE")
