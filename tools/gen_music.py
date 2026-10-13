#!/usr/bin/env python3
"""
Generador de música para LATINDRIVER (autocontenido).
Estilo: Mega Man + tropical (merengue, bachata, cumbia digital).
Usa mido directamente sin dependencias externas.
"""

import mido
from mido import MidiFile, MidiTrack, Message, MetaMessage
import os
import subprocess

from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
OUTPUT_DIR = PROJECT_ROOT / "assets" / "audio" / "music"
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

TPQ = 480
SOUNDFONT = "/usr/share/sounds/sf3/MuseScore_General.sf3"

INST = {
    'piano': 0, 'violin': 40, 'cello': 42, 'contrabass': 43,
    'flute': 73, 'oboe': 68, 'clarinet': 71, 'bassoon': 70,
    'french_horn': 60, 'trumpet': 56, 'trombone': 57, 'tuba': 58,
    'timpani': 47, 'harp': 46, 'strings': 48, 'choir': 52,
}

def note_to_midi(note_str):
    notes = {'C': 0, 'C#': 1, 'Db': 1, 'D': 2, 'D#': 3, 'Eb': 3,
             'E': 4, 'F': 5, 'F#': 6, 'Gb': 6, 'G': 7, 'G#': 8,
             'Ab': 8, 'A': 9, 'A#': 10, 'Bb': 10, 'B': 11}
    if len(note_str) >= 2 and note_str[-1].isdigit():
        note, octave = note_str[:-1], int(note_str[-1])
    else:
        note, octave = note_str, 4
    return (octave + 1) * 12 + notes[note]

def create_track(name, program, channel, notes_data):
    track = MidiTrack()
    track.append(MetaMessage('track_name', name=name, time=0))
    track.append(Message('program_change', program=program, channel=channel, time=0))
    events = []
    for note, vel, start, dur in notes_data:
        start_tick = int(start * TPQ)
        end_tick = int((start + dur) * TPQ)
        events.append((start_tick, 'on', note, vel))
        events.append((end_tick, 'off', note, 0))
    events.sort(key=lambda e: (e[0], 0 if e[1] == 'off' else 1))
    current_tick = 0
    for tick, event_type, pitch, velocity in events:
        delta = max(0, tick - current_tick)
        if event_type == 'on':
            track.append(Message('note_on', note=pitch, velocity=velocity, channel=channel, time=delta))
        else:
            track.append(Message('note_off', note=pitch, velocity=0, channel=channel, time=delta))
        current_tick = tick
    track.append(MetaMessage('end_of_track', time=0))
    return track

def render_wav(mid_path):
    wav_path = mid_path.replace('.mid', '.wav')
    cmd = ['/usr/bin/fluidsynth', '-ni', '-F', wav_path, '-r', '44100', '-g', '0.4', '-z', '64', SOUNDFONT, mid_path]
    subprocess.run(cmd, capture_output=True, timeout=120)
    return wav_path

def convert_ogg(wav_path):
    ogg_path = wav_path.replace('.wav', '.ogg')
    cmd = ['/usr/bin/ffmpeg', '-y', '-i', wav_path, '-af', 'loudnorm=I=-16:TP=-1.5:LRA=11', '-c:a', 'libvorbis', '-q:a', '5', ogg_path]
    subprocess.run(cmd, capture_output=True, timeout=300)
    return ogg_path

# Progresiones tropicales
PROG = [
    [60, 64, 67, 72],  # C
    [57, 60, 64, 69],  # Am
    [55, 59, 62, 67],  # G
    [50, 53, 57, 62],  # Dm
]

def gen_menu():
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 90
    piano = []
    for bar in range(16):
        root = PROG[bar % 4][0]
        for beat in range(4):
            piano.extend([(root+12, 30, bar*4+beat, 0.5), (root+16, 25, bar*4+beat+0.5, 0.5),
                         (root+19, 28, bar*4+beat+1, 0.5), (root+24, 32, bar*4+beat+1.5, 0.5)])
    mid.tracks.append(create_track("Piano", INST['piano'], 0, piano))
    bass = []
    for bar in range(16):
        root = PROG[bar % 4][0] - 12
        bass.extend([(root, 40, bar*4, 2), (root+7, 35, bar*4+2, 2)])
    mid.tracks.append(create_track("Bass", INST['contrabass'], 1, bass))
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    return mid

def gen_gameplay():
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 140
    lead = []
    for bar in range(32):
        root = PROG[bar % 4][0]
        pattern = [0, 4, 7, 12, 7, 4, 7, 12]
        for i, p in enumerate(pattern):
            lead.append((root+p, 60, bar*4+i*0.5, 0.4))
    mid.tracks.append(create_track("Lead", INST['trumpet'], 0, lead))
    bass = []
    for bar in range(32):
        root = PROG[bar % 4][0] - 12
        for i in range(8):
            bass.append((root, 50, bar*4+i*0.5, 0.4))
    mid.tracks.append(create_track("Bass", INST['contrabass'], 1, bass))
    perc = []
    for bar in range(32):
        for beat in range(4):
            perc.extend([(36, 60, bar*4+beat, 0.25), (38, 50, bar*4+beat+0.5, 0.25)])
    mid.tracks.append(create_track("Perc", 0, 9, perc))
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    return mid

def gen_gameover():
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 60
    piano = []
    for bar in range(8):
        root = PROG[bar % 4][0]
        piano.extend([(root, 40, bar*4, 2), (root+7, 35, bar*4+2, 2), (root+12, 30, bar*4+3, 1)])
    mid.tracks.append(create_track("Piano", INST['piano'], 0, piano))
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    return mid

print("Generando música para LATINDRIVER (Mega Man + tropical)...")
print("\n[1/3] Menú...")
mid1 = gen_menu()
p1 = os.path.join(OUTPUT_DIR, "menu.mid")
mid1.save(p1)
ogg1 = convert_ogg(render_wav(p1))
print(f"  ✓ {ogg1}")

print("\n[2/3] Gameplay...")
mid2 = gen_gameplay()
p2 = os.path.join(OUTPUT_DIR, "gameplay.mid")
mid2.save(p2)
ogg2 = convert_ogg(render_wav(p2))
print(f"  ✓ {ogg2}")

print("\n[3/3] Game Over...")
mid3 = gen_gameover()
p3 = os.path.join(OUTPUT_DIR, "gameover.mid")
mid3.save(p3)
ogg3 = convert_ogg(render_wav(p3))
print(f"  ✓ {ogg3}")

print(f"\n✅ Música generada en: {OUTPUT_DIR}")
