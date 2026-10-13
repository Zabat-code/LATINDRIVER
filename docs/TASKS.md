# LATINDRIVER — Tareas (Fase 1: Prototipo)

Instrucciones para el constructor: sigue las tareas en orden. Marca cada una como hecha antes de pasar a la siguiente. Si una tarea depende de una decisión no cubierta en PROJECT_CONTEXT.md, pregunta antes de improvisar.

## 0. Setup del proyecto
- [ ] Confirmar que Godot 4.x (.NET) está instalado y abre correctamente
- [ ] Crear nuevo proyecto Godot en `LATINDRIVER`, modo "C#" (Mono/.NET)
- [ ] Crear la estructura de carpetas descrita en PROJECT_CONTEXT.md (`scenes/`, `scripts/`, `assets/`, `docs/`)
- [ ] Copiar `PROJECT_CONTEXT.md` y `TASKS.md` a `LATINDRIVER/docs/`
- [ ] Inicializar repo Git en la carpeta del proyecto, con `.gitignore` estándar de Godot (incluir `.godot/`, `*.tmp`, etc.)
- [ ] Hacer commit inicial

## 1. Escena Player (placeholder)
- [ ] Crear `Player.tscn`: un `CharacterBody2D` con un `Sprite2D` (usar un `ColorRect` o `Sprite2D` con textura de un cuadrado de color como placeholder, ej. 32x32 naranja) y un `CollisionShape2D`
- [ ] Crear `Player.cs`:
  - Movimiento automático hacia adelante en el eje X a velocidad constante (variable pública `Speed`, valor inicial sugerido: 200 px/s)
  - Input de salto: al presionar una tecla (ej. Espacio o flecha arriba), aplicar impulso vertical simulando salto por encima de obstáculos bajos
  - Input de agachar/deslizar: al presionar otra tecla (ej. flecha abajo), reducir temporalmente el `CollisionShape2D` para pasar por debajo de obstáculos altos
  - Detección de colisión con obstáculos: si colisiona con un nodo del grupo `"obstacle"`, emitir una señal `PlayerHit`

## 2. Sistema de nivel y scroll
- [ ] Crear `Level.tscn` con un fondo simple (puede ser un `ColorRect` gris representando el asfalto, y otro de otro color para el "cielo"/fondo)
- [ ] Decidir enfoque de scroll: **mover la cámara hacia adelante** (más simple) en vez de mover el mundo. La `Camera2D` sigue al jugador en X automáticamente
- [ ] Crear `LevelGenerator.cs`:
  - Genera obstáculos a intervalos (aleatorios dentro de un rango, ej. cada 3-6 segundos de scroll)
  - Por ahora, un solo tipo de obstáculo placeholder (`ColorRect` rojo) que se puede saltar
  - Sistema de "pool" simple o instanciación directa (no optimizar prematuramente, es prototipo)

## 3. GameManager y HUD básico
- [ ] Crear `GameManager.cs` (autoload/singleton en Godot):
  - Mantiene referencia al modo de juego activo
  - Escucha la señal `PlayerHit` del jugador y decide qué pasa (perder vida, game over, etc. — definir en base al modo)
- [ ] Crear `HUD.tscn` + `HUD.cs`:
  - Label de distancia recorrida (contador simple basado en posición X del jugador)
  - Label de estado ("Jugando", "Game Over", "¡Meta!")

## 4. Implementar modo "Contra reloj" (primer modo, el más simple)
- [ ] Crear interfaz `IGameMode.cs` con los métodos descritos en PROJECT_CONTEXT.md
- [ ] Crear `TimeAttackMode.cs`:
  - Timer descendente (valor inicial sugerido: 60 segundos)
  - `CheckLoseCondition()`: retorna true si el timer llega a 0
  - `CheckWinCondition()`: retorna true si el jugador alcanza una posición X objetivo (fin del nivel)
- [ ] Conectar el HUD para mostrar el timer
- [ ] Al perder o ganar, mostrar mensaje simple y opción de reiniciar (puede ser con `get_tree().reload_current_scene()`)

## 5. Menú principal mínimo
- [ ] Crear `Main.tscn`: pantalla simple con un botón "Jugar" que carga `Level.tscn` en modo "Contra reloj" (los otros modos se añaden después)

## 6. Prueba y ajuste
- [ ] Verificar que el ciclo completo funciona: menú → nivel → jugar → perder o ganar → volver a intentar
- [ ] Anotar cualquier bug o duda en `docs/NOTES.md` (crear este archivo si no existe) para revisión del arquitecto

---

## Fase 2 (NO empezar todavía — solo referencia futura)
- Implementar `ChaseMode.cs` (perros persiguiendo)
- Implementar `FuelMode.cs` (tanque de gasolina + bidones recolectables)
- Reemplazar placeholders con pixel art real cuando el usuario lo entregue
- Integrar música y SFX cuando estén listos (formato `.ogg`)
- Sistema de guardado de mejores puntuaciones/tiempos (JSON o ConfigFile)
