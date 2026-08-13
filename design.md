# Design.md — LATINDRIVER (Godot 4.7 + C#)

Dirección visual: **"Concert Hall"** — misma que el rediseño de MidiMate, para
que tus apps se vean como una familia. Profundidad, latón pulido, acento de
energía cian. Tipografía con carácter.

En Godot no hay CSS; se aplica vía **Theme resources (.tres)** y fuentes. Este
archivo es la dirección; `ConcertHall.tres` es la implementación.

## Tokens (convertidos a Color 0-1 para Godot)
- bg-void    #0e0c16 → (0.055, 0.047, 0.086)
- bg-panel   #171427 → (0.090, 0.078, 0.153)
- bg-elev    #2c2547 → (0.173, 0.145, 0.278)
- brass      #d4a843 → (0.831, 0.659, 0.263)
- brass-soft #f0cd73 → (0.941, 0.804, 0.451)
- ivory      #f4eede → (0.957, 0.933, 0.871)
- ivory-dim  #9b9384 → (0.608, 0.576, 0.518)
- accent     #54d6c4 → (0.329, 0.839, 0.769)
- wrong      #e8605c → (0.910, 0.378, 0.361)

## Tipografía
- Display: Cormorant Garamond (o serif del sistema). En Godot: FontFile .ttf/.otf.
- UI: Inter. Mismo fallback que MidiMate.
- Jerarquía: título 22px / 700, sección 17px / 600, cuerpo 13px.

## Aplicación
- `ConcertHall.tres`: Theme con default_base_color = bg-panel, accent para
  focos/botones activos, font por defecto = Inter.
- Asignar el Theme al nodo raíz (ProjectSettings o nodo Control raíz).
- Para el HUD del runner: usar bg-elev en paneles, brass en texto de score,
  accent en el indicador de "combo/energía".

## Notas
- Build headless debe seguir en 0 errores tras aplicar el Theme.
- El Theme es un asset; no rompe la lógica C#.
- Ver design.md de MidiMate para la filosofía completa.
