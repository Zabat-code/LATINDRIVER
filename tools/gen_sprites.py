#!/usr/bin/env python3
"""
Generador de sprites pixel art 64x64 para LATINDRIVER.
Estilo: pixel art con paleta tropical (influencia Mega Man + tropical).
"""

from PIL import Image, ImageDraw
import os

from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
OUTPUT_DIR = PROJECT_ROOT / "assets" / "sprites"
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

# Paleta tropical
COLORS = {
    'bg': (0, 0, 0, 0),           # transparente
    'black': (20, 20, 30, 255),
    'white': (255, 255, 255, 255),
    'red': (220, 60, 60, 255),
    'orange': (255, 140, 40, 255),
    'yellow': (255, 210, 60, 255),
    'green': (60, 180, 80, 255),
    'teal': (40, 180, 170, 255),
    'blue': (50, 100, 200, 255),
    'purple': (140, 60, 180, 255),
    'brown': (140, 90, 50, 255),
    'skin': (255, 200, 150, 255),
    'dark': (40, 40, 50, 255),
    'gray': (120, 120, 130, 255),
    'light_gray': (180, 180, 190, 255),
    'road': (80, 80, 90, 255),
    'road_line': (200, 200, 80, 255),
    'grass': (80, 160, 60, 255),
    'building': (200, 180, 160, 255),
    'window': (100, 150, 200, 255),
    'door': (120, 80, 40, 255),
    'roof': (180, 60, 40, 255),
    'tree': (40, 120, 40, 255),
    'trunk': (100, 70, 30, 255),
    'car_body': (200, 50, 50, 255),
    'car_window': (150, 200, 220, 255),
    'car_wheel': (30, 30, 30, 255),
    'moto_body': (50, 150, 200, 255),
    'moto_wheel': (30, 30, 30, 255),
    'moto_rider': (255, 200, 150, 255),
    'moto_helmet': (220, 60, 60, 255),
    'dog_body': (160, 120, 60, 255),
    'dog_ear': (120, 80, 40, 255),
    'dog_nose': (40, 30, 20, 255),
    'cone': (255, 100, 30, 255),
    'cone_white': (255, 255, 255, 255),
    'barrel': (180, 120, 40, 255),
    'barrel_band': (220, 60, 40, 255),
    'fuel_can': (220, 60, 40, 255),
    'fuel_handle': (180, 40, 30, 255),
    'truck_body': (180, 180, 190, 255),
    'truck_cargo': (140, 140, 150, 255),
    'truck_wheel': (30, 30, 30, 255),
    'jeep_body': (100, 140, 80, 255),
    'jeep_wheel': (30, 30, 30, 255),
    'animal_body': (200, 180, 140, 255),
    'animal_leg': (160, 140, 100, 255),
}

def create_image(w=64, h=64):
    return Image.new('RGBA', (w, h), COLORS['bg'])

def save(img, name):
    img.save(os.path.join(OUTPUT_DIR, name))
    print(f"  ✓ {name}")

# === MOTOCICLETA (vista lateral, 64x64) ===
def gen_moto():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Rueda trasera
    d.ellipse([8, 40, 28, 60], fill=COLORS['moto_wheel'])
    d.ellipse([12, 44, 24, 56], fill=COLORS['gray'])
    
    # Rueda delantera
    d.ellipse([40, 40, 60, 60], fill=COLORS['moto_wheel'])
    d.ellipse([44, 44, 56, 56], fill=COLORS['gray'])
    
    # Cuerpo de la moto
    d.rectangle([20, 35, 45, 45], fill=COLORS['moto_body'])
    d.rectangle([25, 30, 40, 38], fill=COLORS['teal'])
    
    # Asiento
    d.rectangle([15, 28, 30, 35], fill=COLORS['dark'])
    
    # Manubrio
    d.line([45, 35, 55, 25], fill=COLORS['gray'], width=3)
    d.line([55, 25, 58, 20], fill=COLORS['gray'], width=2)
    
    # Piloto (cuerpo)
    d.rectangle([22, 15, 35, 30], fill=COLORS['blue'])
    
    # Casco
    d.ellipse([20, 5, 35, 20], fill=COLORS['moto_helmet'])
    d.rectangle([25, 12, 35, 18], fill=COLORS['white']) # visor
    
    # Brazos
    d.line([30, 20, 45, 30], fill=COLORS['skin'], width=3)
    
    # Piernas
    d.line([25, 30, 20, 40], fill=COLORS['dark'], width=3)
    d.line([30, 30, 35, 40], fill=COLORS['dark'], width=3)
    
    save(img, "moto1.png")

# === PERRO (vista lateral, 64x64) ===
def gen_dog():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([15, 30, 50, 45], fill=COLORS['dog_body'])
    
    # Cabeza
    d.rectangle([45, 20, 60, 38], fill=COLORS['dog_body'])
    
    # Orejas
    d.polygon([(48, 20), (52, 10), (56, 20)], fill=COLORS['dog_ear'])
    d.polygon([(54, 20), (58, 10), (62, 20)], fill=COLORS['dog_ear'])
    
    # Ojos
    d.rectangle([50, 25, 54, 29], fill=COLORS['black'])
    d.rectangle([56, 25, 60, 29], fill=COLORS['black'])
    
    # Nariz
    d.rectangle([58, 32, 62, 36], fill=COLORS['dog_nose'])
    
    # Patas
    d.rectangle([18, 45, 22, 58], fill=COLORS['dog_body'])
    d.rectangle([28, 45, 32, 58], fill=COLORS['dog_body'])
    d.rectangle([38, 45, 42, 58], fill=COLORS['dog_body'])
    d.rectangle([46, 45, 50, 58], fill=COLORS['dog_body'])
    
    # Cola
    d.line([15, 35, 5, 25], fill=COLORS['dog_body'], width=4)
    
    save(img, "dog.png")

# === CONO (64x64) ===
def gen_cone():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Base
    d.rectangle([10, 55, 54, 62], fill=COLORS['cone'])
    
    # Cuerpo del cono
    d.polygon([(32, 5), (15, 55), (49, 55)], fill=COLORS['cone'])
    
    # Franja blanca
    d.polygon([(32, 25), (22, 50), (42, 50)], fill=COLORS['cone_white'])
    
    save(img, "cone.png")

# === BARRIL (64x64) ===
def gen_barrel():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([15, 10, 49, 58], fill=COLORS['barrel'])
    
    # Bandas
    d.rectangle([15, 20, 49, 25], fill=COLORS['barrel_band'])
    d.rectangle([15, 40, 49, 45], fill=COLORS['barrel_band'])
    
    # Tapa
    d.ellipse([18, 5, 46, 15], fill=COLORS['dark'])
    
    save(img, "barrel.png")

# === CARRO (vista lateral, 64x64) ===
def gen_car():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([5, 30, 59, 48], fill=COLORS['car_body'])
    
    # Cabina
    d.rectangle([15, 18, 45, 32], fill=COLORS['car_body'])
    
    # Ventanas
    d.rectangle([18, 20, 30, 30], fill=COLORS['car_window'])
    d.rectangle([33, 20, 43, 30], fill=COLORS['car_window'])
    
    # Ruedas
    d.ellipse([10, 42, 24, 56], fill=COLORS['car_wheel'])
    d.ellipse([40, 42, 54, 56], fill=COLORS['car_wheel'])
    d.ellipse([14, 46, 20, 52], fill=COLORS['gray'])
    d.ellipse([44, 46, 50, 52], fill=COLORS['gray'])
    
    # Faros
    d.rectangle([55, 35, 59, 40], fill=COLORS['yellow'])
    
    save(img, "car.png")

# === CAMIONETA (vista lateral, 64x64) ===
def gen_truck():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cabina
    d.rectangle([5, 20, 25, 48], fill=COLORS['truck_body'])
    d.rectangle([8, 22, 22, 32], fill=COLORS['car_window'])
    
    # Carga
    d.rectangle([25, 15, 59, 48], fill=COLORS['truck_cargo'])
    
    # Ruedas
    d.ellipse([8, 42, 22, 56], fill=COLORS['truck_wheel'])
    d.ellipse([45, 42, 59, 56], fill=COLORS['truck_wheel'])
    d.ellipse([12, 46, 18, 52], fill=COLORS['gray'])
    d.ellipse([49, 46, 55, 52], fill=COLORS['gray'])
    
    save(img, "truck.png")

# === JEEPETA (vista lateral, 64x64) ===
def gen_jeep():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([5, 25, 59, 48], fill=COLORS['jeep_body'])
    
    # Capota
    d.rectangle([10, 15, 50, 28], fill=COLORS['jeep_body'])
    
    # Ventanas
    d.rectangle([13, 17, 28, 26], fill=COLORS['car_window'])
    d.rectangle([31, 17, 46, 26], fill=COLORS['car_window'])
    
    # Ruedas grandes
    d.ellipse([8, 40, 26, 58], fill=COLORS['jeep_wheel'])
    d.ellipse([38, 40, 56, 58], fill=COLORS['jeep_wheel'])
    d.ellipse([13, 45, 21, 53], fill=COLORS['gray'])
    d.ellipse([43, 45, 51, 53], fill=COLORS['gray'])
    
    # Parrilla
    d.rectangle([55, 30, 59, 45], fill=COLORS['gray'])
    
    save(img, "jeep.png")

# === BIDÓN DE GASOLINA (64x64) ===
def gen_fuel():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([20, 15, 44, 55], fill=COLORS['fuel_can'])
    
    # Asa
    d.rectangle([25, 8, 39, 15], fill=COLORS['fuel_handle'])
    
    # Tapa
    d.rectangle([28, 12, 36, 18], fill=COLORS['dark'])
    
    # Etiqueta
    d.rectangle([24, 30, 40, 45], fill=COLORS['yellow'])
    d.text((28, 35), "GAS", fill=COLORS['black'])
    
    save(img, "fuel.png")

# === ANIMAL (vaca/caballo, 64x64) ===
def gen_animal():
    img = create_image()
    d = ImageDraw.Draw(img)
    
    # Cuerpo
    d.rectangle([10, 25, 50, 45], fill=COLORS['animal_body'])
    
    # Cabeza
    d.rectangle([45, 15, 60, 35], fill=COLORS['animal_body'])
    
    # Orejas
    d.polygon([(48, 15), (50, 8), (52, 15)], fill=COLORS['animal_leg'])
    d.polygon([(54, 15), (56, 8), (58, 15)], fill=COLORS['animal_leg'])
    
    # Ojos
    d.rectangle([50, 20, 54, 24], fill=COLORS['black'])
    
    # Nariz
    d.rectangle([55, 28, 60, 33], fill=COLORS['dark'])
    
    # Patas
    d.rectangle([12, 45, 16, 60], fill=COLORS['animal_leg'])
    d.rectangle([22, 45, 26, 60], fill=COLORS['animal_leg'])
    d.rectangle([32, 45, 36, 60], fill=COLORS['animal_leg'])
    d.rectangle([42, 45, 46, 60], fill=COLORS['animal_leg'])
    
    # Cola
    d.line([10, 30, 3, 20], fill=COLORS['animal_leg'], width=3)
    
    save(img, "animal.png")

# === FONDOS (1152x648) ===
def gen_background(name, sky_color, building_color, road_color, turn):
    img = Image.new('RGB', (1152, 648), sky_color)
    d = ImageDraw.Draw(img)
    
    # Cielo con gradiente
    for y in range(400):
        r = int(sky_color[0] * (1 - y/500))
        g = int(sky_color[1] * (1 - y/500))
        b = int(sky_color[2] * (1 - y/500))
        d.line([(0, y), (1152, y)], fill=(r, g, b))
    
    # Edificios
    import random
    random.seed(42)
    x = 0
    while x < 1152:
        w = random.randint(80, 150)
        h = random.randint(150, 350)
        d.rectangle([x, 400-h, x+w, 400], fill=building_color)
        
        # Ventanas
        for wy in range(400-h+20, 380, 30):
            for wx in range(x+10, x+w-10, 25):
                if random.random() > 0.3:
                    d.rectangle([wx, wy, wx+15, wy+20], fill=COLORS['window'])
        
        x += w + random.randint(10, 30)
    
    # Carretera
    d.rectangle([0, 400, 1152, 648], fill=road_color)
    
    # Líneas de carril (4 carriles)
    for i in range(1, 4):
        y = 400 + i * 62
        for x in range(0, 1152, 40):
            d.rectangle([x, y, x+20, y+4], fill=COLORS['road_line'])
    
    # Acera
    d.rectangle([0, 390, 1152, 400], fill=COLORS['light_gray'])
    
    img.save(os.path.join(OUTPUT_DIR, name))
    print(f"  ✓ {name}")

# Generar todo
print("Generando sprites pixel art 64x64...")
gen_moto()
gen_dog()
gen_cone()
gen_barrel()
gen_car()
gen_truck()
gen_jeep()
gen_fuel()
gen_animal()

print("\nGenerando fondos 1152x648...")
gen_background("bg_colonial_dia.png", (135, 206, 235), (220, 200, 180), (80, 80, 90), "dia")
gen_background("bg_colonial_noche.png", (20, 20, 40), (60, 50, 50), (40, 40, 50), "noche")
gen_background("bg_barrio_dia.png", (135, 206, 235), (180, 160, 140), (70, 70, 80), "dia")
gen_background("bg_avenida_dia.png", (135, 206, 235), (200, 200, 210), (90, 90, 100), "dia")
gen_background("bg_poblado_dia.png", (135, 206, 235), (160, 140, 120), (60, 60, 70), "dia")

print("\n✅ Todos los sprites generados en:", OUTPUT_DIR)
