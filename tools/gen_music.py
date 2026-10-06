#!/usr/bin/env python3
"""
Generador de música para LATINDRIVER.
Estilo: Mega Man + tropical (merengue, bachata, cumbia digital).
Usa midi-orquesta para generar las pistas.
"""

import sys
sys.path.insert(0, "/home/gerardo/Desktop")

from compositor_avanzado import (
    create_track, note_to_midi, render_wav, convert_ogg,
    INST, TPQ, MidiFile, MetaMessage
)
import mido
import os

OUTPUT_DIR = "/home/gerardo/Desktop/LATINDRIVER/assets/audio/music"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Escalas tropicales
C_MAJOR = [60, 62, 64, 65, 67, 69, 71]
A_MINOR = [57, 59, 60, 62, 64, 65, 67]
G_MAJOR = [55, 57, 59, 60, 62, 64, 66]
D_MINOR = [50, 52, 53, 55, 57, 58, 60]

# Progresiones tropicales
PROG_TROPICAL = [
    [60, 64, 67, 72],  # C
    [57, 60, 64, 69],  # Am
    [55, 59, 62, 67],  # G
    [50, 53, 57, 62],  # Dm
]

def gen_menu_music():
    """Música del menú principal: suave, tropical, acogedora"""
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 90
    
    # Piano: arpegios suaves
    piano_notes = []
    for bar in range(16):
        root = PROG_TROPICAL[bar % 4][0]
        for beat in range(4):
            piano_notes.append((root + 12, 30, bar * 4 + beat, 0.5))
            piano_notes.append((root + 16, 25, bar * 4 + beat + 0.5, 0.5))
            piano_notes.append((root + 19, 28, bar * 4 + beat + 1, 0.5))
            piano_notes.append((root + 24, 32, bar * 4 + beat + 1.5, 0.5))
    mid.tracks.append(create_track("Piano", INST['piano'], 0, piano_notes))
    
    # Guitarra: rasgueo suave
    guitar_notes = []
    for bar in range(16):
        root = PROG_TROPICAL[bar % 4][0]
        for beat in range(4):
            guitar_notes.append((root, 35, bar * 4 + beat, 1))
            guitar_notes.append((root + 7, 30, bar * 4 + beat + 0.5, 0.5))
    mid.tracks.append(create_track("Guitar", INST['clarinet'], 1, guitar_notes))
    
    # Bajo: notas graves
    bass_notes = []
    for bar in range(16):
        root = PROG_TROPICAL[bar % 4][0] - 12
        bass_notes.append((root, 40, bar * 4, 2))
        bass_notes.append((root + 7, 35, bar * 4 + 2, 2))
    mid.tracks.append(create_track("Bass", INST['contrabass'], 2, bass_notes))
    
    # Percusión: suave
    perc_notes = []
    for bar in range(16):
        for beat in range(4):
            perc_notes.append((36, 30, bar * 4 + beat, 0.25))
            perc_notes.append((38, 25, bar * 4 + beat + 0.5, 0.25))
    mid.tracks.append(create_track("Percussion", 0, 9, perc_notes))
    
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    
    return mid, bpm

def gen_gameplay_music():
    """Música de gameplay: energética, Mega Man + tropical"""
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 140
    
    # Lead: melodía energética estilo Mega Man
    lead_notes = []
    for bar in range(32):
        root = PROG_TROPICAL[bar % 4][0]
        # Patrón de 8 notas por compás
        pattern = [0, 4, 7, 12, 7, 4, 7, 12]
        for i, p in enumerate(pattern):
            lead_notes.append((root + p, 60, bar * 4 + i * 0.5, 0.4))
    mid.tracks.append(create_track("Lead", INST['trumpet'], 0, lead_notes))
    
    # Bajo: patrón de 8 notas
    bass_notes = []
    for bar in range(32):
        root = PROG_TROPICAL[bar % 4][0] - 12
        for i in range(8):
            bass_notes.append((root, 50, bar * 4 + i * 0.5, 0.4))
    mid.tracks.append(create_track("Bass", INST['contrabass'], 1, bass_notes))
    
    # Percusión: patrón tropical
    perc_notes = []
    for bar in range(32):
        for beat in range(4):
            perc_notes.append((36, 60, bar * 4 + beat, 0.25))
            perc_notes.append((38, 50, bar * 4 + beat + 0.5, 0.25))
            perc_notes.append((42, 40, bar * 4 + beat + 0.25, 0.25))
    mid.tracks.append(create_track("Percussion", 0, 9, perc_notes))
    
    # Arpa: acompañamiento
    harp_notes = []
    for bar in range(32):
        root = PROG_TROPICAL[bar % 4][0]
        for beat in range(4):
            harp_notes.append((root + 12, 35, bar * 4 + beat, 0.5))
            harp_notes.append((root + 19, 30, bar * 4 + beat + 0.5, 0.5))
    mid.tracks.append(create_track("Harp", INST['harp'], 2, harp_notes))
    
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    
    return mid, bpm

def gen_gameover_music():
    """Música de game over: triste, lenta"""
    mid = MidiFile(ticks_per_beat=TPQ)
    bpm = 60
    
    # Piano: notas lentas y tristes
    piano_notes = []
    for bar in range(8):
        root = PROG_TROPICAL[bar % 4][0]
        piano_notes.append((root, 40, bar * 4, 2))
        piano_notes.append((root + 7, 35, bar * 4 + 2, 2))
        piano_notes.append((root + 12, 30, bar * 4 + 3, 1))
    mid.tracks.append(create_track("Piano", INST['piano'], 0, piano_notes))
    
    # Cuerdas: sostenidos
    strings_notes = []
    for bar in range(8):
        root = PROG_TROPICAL[bar % 4][0]
        strings_notes.append((root, 25, bar * 4, 4))
        strings_notes.append((root + 7, 20, bar * 4, 4))
    mid.tracks.append(create_track("Strings", INST['strings'], 1, strings_notes))
    
    mid.tracks[0].insert(0, MetaMessage('set_tempo', tempo=mido.bpm2tempo(bpm), time=0))
    mid.tracks[0].insert(0, MetaMessage('time_signature', numerator=4, denominator=4, time=0))
    
    return mid, bpm

# Generar todo
print("Generando música para LATINDRIVER...")
print("Estilo: Mega Man + tropical")

print("\n[1/3] Menú principal...")
mid1, bpm1 = gen_menu_music()
path1 = os.path.join(OUTPUT_DIR, "menu.ogg")
mid1.save(path1.replace('.ogg', '.mid'))
wav1 = render_wav(path1.replace('.ogg', '.mid'))
ogg1 = convert_ogg(wav1)
print(f"  ✓ {ogg1}")

print("\n[2/3] Gameplay...")
mid2, bpm2 = gen_gameplay_music()
path2 = os.path.join(OUTPUT_DIR, "gameplay.ogg")
mid2.save(path2.replace('.ogg', '.mid'))
wav2 = render_wav(path2.replace('.ogg', '.mid'))
ogg2 = convert_ogg(wav2)
print(f"  ✓ {ogg2}")

print("\n[3/3] Game Over...")
mid3, bpm3 = gen_gameover_music()
path3 = os.path.join(OUTPUT_DIR, "gameover.ogg")
mid3.save(path3.replace('.ogg', '.mid'))
wav3 = render_wav(path3.replace('.ogg', '.mid'))
ogg3 = convert_ogg(wav3)
print(f"  ✓ {ogg3}")

print("\n✅ Música generada en:", OUTPUT_DIR)
