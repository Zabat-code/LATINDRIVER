# LATINDRIVER — Notas del prototipo Fase 1 (revisión del arquitecto)

## Estado verificado (compilación + runtime)
- `dotnet build` contra `Godot.NET.Sdk/4.7.1` (net8.0): **Build succeeded, 0 Error(s)**.
  (algunos warnings de build del SDK de Godot, no son CS y son benignos).
- **Runtime headless REAL** (`Godot_v4.7.1 ... --headless --path .`):
  - Level.tscn y Main.tscn cargan y ejecutan `_Ready`/`_Process` sin errores de
    juego (0 `script error` / `nullreference` / `parse error` / `node not found`).
  - Los `ERROR: BUG: Unreferenced static string` / `RID allocations ... leaked at
    exit` que aparecen al cerrar son ruido del shutdown del motor en modo headless,
    NO errores del juego.
- Limitación persistente: **no hay playtest visual** (sin GUI de Godot en este
  entorno). Lo verificado es carga + ejecución de frame, no "sentir" el salto.

## Bugs de Fase 1 corregidos en esta sesión
1. **HUD pisaba líneas:** `SetTime`/`SetStatus` sobreescribían el texto completo,
   así que solo se veía la última línea. Ahora HUD mantiene 3 líneas separadas
   (Distancia / Tiempo / Estado) y cada setter actualiza solo su línea.
2. **Fondo se quedaba fijo:** el Sprite2D `Background` estaba anclado en X=0, así
   que a pocos segundos se veía el vacío a la derecha. Ahora `Level.cs` reposiciona
   el fondo cada frame para que su borde izquierdo coincida con el viewport de la
   cámara del jugador.
3. **Suelo y colisiones cortos:** `Ground` (ColorRect) y el `StaticBody2D` del piso
   estaban en X=2000/4000px; el jugador los rebasaba rápido. Ahora suelo y piso
   arrancan en X=0 y se extienden ~20000px (suficiente para GoalPositionX=6000).
4. **Obstáculos nunca se destruían:** el Level ahora hace `QueueFree()` de los
   obstáculos que quedaron >800px atrás del jugador (evita fuga de memoria entre
   partidas / reinicios con R).
5. **Meta muy corta:** `GoalPositionX` subió de 3000 a 6000 (la moto a 200px/s
   llega en ~30s; con timer de 60s queda un reto razonable). Pendiente ajustar
   cuando se defina el "mapa real".

## Decisiones de diseño (alineadas a TASKS.md / PROJECT_CONTEXT.md)
- Player.tscn escena independiente; Level instancia Player.tscn + HUD.tscn.
- Obstáculo = `StaticBody2D` en grupo `"obstacle"`; el CharacterBody2D del jugador
  registra slide-collision y Player.cs emite `PlayerHit`, que GameManager escucha.
- Cámara: `Camera2D` hijo del Player (sigue en X).
- `Level.cs` (nuevo) orquesta fondo + cleanup de obstáculos; NO contiene lógica de
  modo de juego (esa vive en GameManager / modos).

## Mecánicas implementadas (Fase 1, estilo Dead Ahead)
- Avance automático hacia la derecha (Speed 200 px/s); la cámara sigue en X.
- **3 carriles laterales** (posiciones Y fijas 470/510/550 sobre la carretera).
  Cambio con flechas IZQUIERDA/DERECHA (`ui_left`/`ui_right`), lerp suave en Y.
- Salto (`ui_up`, solo en suelo) y agachado/desliz (`ui_down`) para esquivar.
- Colisión con obstáculo (grupo "obstacle") → señal `PlayerHit` → GameManager
  termina la partida. Reinicio con R.
- **Bug "flecha izquierda desaparece la imagen"**: en el código previo no había
  handler de `ui_left`, y la `Camera2D` era hija del Player (seguía su Y). Al
  moverse en Y el sprite podía salir del viewport. Solución: Camera2D ahora es
  hija de Level y sigue SOLO X con Y fija (400); el Sprite2D del Player usa
  `centered=true`. Así el sprite siempre queda visible al cambiar de carril.

## Fase 2 — modos de juego implementados
- Interfaz `IGameMode` ampliada con `OnPlayerCollected(itemType)` (helper de la
  señal `PlayerCollected` del Player). Los 3 modos la implementan.
- `GameConfig.SelectedMode` (enum: TimeAttack/Chase/Fuel) elegido en el menú y
  leído por GameManager en Setup() para instanciar el modo correcto.
- **TimeAttack** (Contra Reloj): timer descendente + meta por X (6000px).
- **Chase** (Perros): `ChaseMode` + entidad `Dog` (persigue al jugador, velocidad
  creciente). Pierdes si te atrapan (`HasCaught` en ChaseTick de GameManager).
  LevelGenerator spawnea perros detrás del jugador en este modo.
- **Fuel** (Gasolina): `FuelMode` drena combustible con el tiempo; `Pickup`
  (bidón, grupo "pickup") lo recarga al recogerlo (Player emite PlayerCollected
  -> modo recarga). Pierdes si el tanque llega a 0; ganas al llegar a la meta X.
- `LevelGenerator` genera obstáculos + (perros en Chase) + (bidones en Fuel) según
  el modo. `Main.tscn` tiene 3 botones de modo + label + Jugar.
- Verificación headless: Main (3 botones) y Level cargan sin errores de juego;
  cableado Dog/Pickup/botones confirmado. Build 0 errores.

## Fix del carril (sesión -fwon, 10 pasos concluidos)
Síntoma reportado por el usuario: "se ve el placeholder, se agacha pero NO
cambia de carril". Causa raíz y arreglos:
- **Camera2D colgada del Player en Level.tscn**: la cámara heredaba el Y del
  jugador, así que al cambiar de carril la vista se movía con el sprite y el
  cambio era invisible. Se eliminó esa cámara (queda solo la de Level, con Y
  fija en 400 y siguiendo solo X). Ahora el cambio de carril es visible.
- **Pelea gravedad/lerp**: el Player era CharacterBody2D con gravedad global
  que lo bajaba al suelo (~540) mientras el lerp forzaba Y a 510. Reescrito
  Player.cs para usar salto por "altura propia" (sin gravedad global): el
  carril queda en Y fija y estable, el salto es un offset que sube/baja.
- **Colisión Obstacle→Player sorda**: el Obstacle era `Area2D` y usaba
  `area_entered`, que NO detecta un `CharacterBody2D` → el game over nunca
  disparaba. Ahora Obstacle es `StaticBody2D` en grupo `"obstacle"` y el
  Player lo detecta por `GetSlideCollision` y emite `PlayerHit`, que
  GameManager ya escucha (`OnObstacleHitPlayer`).
- **Obstáculos siempre en carril central**: `LevelGenerator` spawneaba en Y
  fijo (512), así que cambiar de carril no servía de esquive. Ahora sale en
  carril aleatorio (440/510/580).
- Carriles separados 70px (440/510/580) para que el cambio sea claramente
  visible.
- Verificación: `dotnet build` 0 errores + headless REAL de MainMenu y Level
  sin errores de juego. **Sin playtest visual** (sin GUI de Godot en este
  entorno): el feel del salto/carril hay que confirmarlo abriendo en el
  editor. Plan de los 10 pasos en `docs/PLAN_10_PASOS.md`.

## Bugs conocidos / dudas para el arquitecto
- En ChaseMode la "victoria" no existe (supervivencia); si se quiere objetivo por
  distancia, añadir `CheckWinCondition` aparte.
- Faltan: sistema de 5 mapas × 3 turnos, audio, arte final (Fase 2 tardía).
- Placeholders de arte/audio pendientes hasta que el usuario entregue assets.
- `IMAGE_PROMPTS.txt` / `IMAGE_PROMPTS_1.txt` están en la raíz (prompts para el
  generador de imágenes); no son parte del build de Godot.
- Verificación sin playtest visual: salto/agachado/carriles/colisión cableados
  pero no "sentidos". Recomendado abrir en editor Godot para validar feel.
