# LATINDRIVER — Plan de 10 pasos (conclusión del prototipo Fase 1+2)

Plan de finalización acordado para dejar el prototipo jugable end-to-end.
El usuario reportó: "se ve el placeholder, se agacha pero NO cambia de carril".
Causa raíz: había una `Camera2D` colgada del Player en Level.tscn que seguía su Y,
así que al cambiar de carril la cámara se movía con el sprite y el cambio era
invisible. Además la colisión Obstacle→Player era sorda y los obstáculos siempre
salían en el carril central.

## Estado por paso
- [x] Paso 1: Diagnosticar bug del carril (cámara hija del Player + separación + colisión sorda)
- [x] Paso 2: Quitar la Camera2D hija del Player en Level.tscn (causa raíz del carril invisible)
- [x] Paso 3: Reescribir Player.cs (salto por altura, carriles separados 70px, agachado sin ocultar)
- [x] Paso 4: Arreglar colisión Obstacle→Player (Obstacle ahora StaticBody2D en grupo "obstacle"; Player la detecta por slide)
- [x] Paso 5: LevelGenerator spawnea obstáculos en carril aleatorio (carril bajo/medio/alto)
- [x] Paso 6: `dotnet build` → 0 errores
- [x] Paso 7: Headless REAL de MainMenu (menú + botones cableados)
- [x] Paso 8: Headless REAL de Level (carga + _Ready/_Process sin errores de juego)
- [x] Paso 9: Limpiar sueltos (.uid/.import/.cs sin trackear) y .gitignore
- [x] Paso 10: Commit local + actualizar NOTES.md con el fix del carril

## Verificación
- Build: `dotnet build` contra Godot.NET.Sdk/4.7.1, net8.0.
- Runtime: `Godot_v4.7.1 ... --headless --path "<native>"` (run real, NO --check-only)
  para MainMenu y Level; se filtra el ruido benigno de shutdown.
- Limitación: sin playtest visual (no hay GUI de Godot en este entorno). Lo verificado
  es carga + ejecución de frame + wiring de colisión/carril, no el "feel" del salto.
