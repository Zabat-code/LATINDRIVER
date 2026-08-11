# LATINDRIVER — Contexto del Proyecto

## Rol de cada agente
- **Arquitecto (Claude/Gerardo):** define estructura, decisiones de diseño, especifica tareas.
- **Constructores (Cline / Hermes Agent):** implementan código siguiendo este documento y las tareas en `TASKS.md`. No deben inventar arquitectura nueva sin consultarla aquí primero. Si algo no está claro, deben preguntar antes de asumir.

## Descripción del juego
LATINDRIVER es un juego de "endless runner" / recorrido horizontal en 2D, donde el jugador controla una moto avanzando por calles y avenidas con temática latinoamericana (asfalto, baches, casas de colores, puestos callejeros, tráfico).

Inspiración de mecánica de scroll y obstáculos: *Dead Ahead: Zombie Warfare* (pero SIN zombies, temática distinta, no copiar assets ni marca).

### Modos de juego (cada uno es una variante de condición de derrota/objetivo, reutilizando el mismo motor de nivel)
1. **Contra reloj:** temporizador descendente. Hay checkpoints en el camino que suman segundos. Pierdes si el timer llega a 0 antes de llegar al destino.
2. **Perros (persecución):** una jauría de perros persigue al jugador desde atrás. Si la distancia entre el jugador y los perros llega a 0, pierdes. La velocidad de los perros varía para generar tensión.
3. **Gasolina:** un tanque que baja constantemente con la distancia recorrida. Hay bidones de gasolina en el camino que rellenan el tanque. Si llega a 0, la moto se detiene y pierdes.

Todos los modos comparten:
- Movimiento automático hacia adelante (scroll)
- Input del jugador: saltar / agacharse / cambiar de carril (a definir en prototipo)
- Obstáculos en el camino (baches, autos parados, animales, etc.)
- Meta: llegar al "destino" (fin de nivel) o sobrevivir el tiempo requerido

## Motor y stack técnico
- **Motor:** Godot 4.x (versión .NET/Mono, para poder usar C#)
- **Lenguaje:** C#
- **Arte:** Pixel art, tamaño de sprite a definir (sugerido 32x32 o 64x64 px). Por ahora se usan placeholders (rectángulos de colores) hasta que el usuario cree el arte final.
- **Audio:** El usuario genera la música con su propio piano virtual (herramienta externa, no parte del código). Los archivos de audio se entregarán en WAV/MP3 y deben convertirse a `.ogg` para uso en Godot (formato recomendado por rendimiento).
- **Control de versiones:** Git (recomendado iniciar un repo local desde el día 1)

## Estructura de carpetas del proyecto Godot
```
LATINDRIVER/
├── project.godot
├── scenes/
│   ├── Main.tscn              # Menú principal
│   ├── Level.tscn             # Escena base de nivel (reutilizable)
│   ├── Player.tscn
│   ├── HUD.tscn
│   └── obstacles/
│       ├── Pothole.tscn
│       ├── ParkedCar.tscn
│       └── FuelCan.tscn
├── scripts/
│   ├── Player.cs
│   ├── LevelGenerator.cs
│   ├── GameManager.cs
│   ├── modes/
│   │   ├── IGameMode.cs        # interfaz común
│   │   ├── TimeAttackMode.cs
│   │   ├── ChaseMode.cs
│   │   └── FuelMode.cs
│   ├── HUD.cs
│   └── obstacles/
│       └── ObstacleBase.cs
├── assets/
│   ├── sprites/                # placeholders por ahora
│   ├── audio/
│   │   ├── music/
│   │   └── sfx/
│   └── fonts/
└── docs/
    ├── PROJECT_CONTEXT.md      # este archivo
    └── TASKS.md
```

## Arquitectura de modos de juego (importante)
Cada modo de juego se implementa como una clase que cumple una interfaz común `IGameMode`, con métodos como:
- `void OnStart()`
- `void OnUpdate(float delta)`
- `bool CheckLoseCondition()`
- `bool CheckWinCondition()`
- `void OnPlayerCollectItem(string itemType)` (ej. gasolina)

El `GameManager` recibe qué modo está activo (seleccionado en el menú) y delega la lógica de victoria/derrota a esa clase, sin necesidad de tocar el resto del juego. Esto permite añadir nuevos modos en el futuro sin reescribir el nivel.

## Convenciones de código
- Nombres de clases y archivos en PascalCase (estándar C#/Godot)
- Comentarios en español está bien, para que el usuario entienda el código
- Cada script debe tener un comentario superior explicando su propósito
- Evitar lógica hardcodeada de un solo modo dentro de Player.cs o LevelGenerator.cs — todo lo específico de modo va en las clases `modes/`

## Sistema de mapas y dificultad (tipo Dead Ahead)

El juego usa **el mismo mapa físico**, pero se juega en 3 turnos (Día/Tarde/Noche) que aumentan la dificultad. La dificultad NO cambia el layout del mapa — cambia la **cantidad y velocidad del tráfico/obstáculos** que aparecen, más la iluminación/paleta visual del turno.

- **Día = fácil:** tráfico ligero, pocos vehículos, ritmo de spawn de obstáculos bajo
- **Tarde = normal:** tráfico moderado, más vehículos concurrentes
- **Noche = difícil:** tráfico denso, mayor velocidad de vehículos, visibilidad reducida (paleta más oscura, se pueden agregar efectos de faros)

Esto se implementa como un multiplicador de "densidad de tráfico" y "velocidad de obstáculos" aplicado sobre la misma tabla de spawn del mapa — NO se rediseña el mapa por turno, solo se ajustan parámetros numéricos + paleta de color/luces.

### Los 5 mapas (basados en lugares reales de República Dominicana, ambientados visualmente con influencia estética mexicana donde haga falta arte de referencia)

Historia y personajes: República Dominicana. Referencias visuales de arte (si escasea recurso RD específico): estética latina genérica/mexicana, adaptando colores y arquitectura a lo dominicano descrito abajo.

1. **Pueblito — basado en Nagua**
   - Ambiente: pueblo pequeño, calles más simples, casas bajas
   - Obstáculos únicos: **animales sueltos en la calle** (gallinas, perros, algún caballo/burro) — fáciles de esquivar, pensados como el mapa introductorio
   - Tráfico bajo por naturaleza (incluso en el turno "noche" es el mapa menos denso de los 5)

2. **Ciudad — estética "Zona Colonial" (Santo Domingo)**
   - Referencias visuales: calles empedradas (adoquines), fachadas coloniales de piedra caliza dorada, balcones de hierro forjado, farolas de calle, mezcla de edificios coloniales con algo de art déco (ej. Calle El Conde)
   - Paleta: tonos cálidos piedra/dorado de día, iluminación de farolas cálida de noche
   - Obstáculos únicos: **carros normales + furgonetas** (camionetas de reparto/guaguas pequeñas) bloqueando carriles

3. **Megaciudad — basado en Gazcue (Santo Domingo)**
   - Ambiente: zona urbana más densa y moderna que la Zona Colonial, edificios de varios pisos, avenidas más anchas
   - Obstáculos únicos: **jeepetas (SUVs/todoterrenos) y camiones largos** — vehículos más grandes que ocupan más espacio/carriles, más difíciles de esquivar o rodear
   - Es el mapa de mayor densidad general de tráfico entre los 5

4. **Ciudad marginada**
   - Ambiente: calles en peor estado, menos infraestructura, más obstáculos "de calle" (baches, cables bajos, basura)
   - Obstáculos únicos: grupos de **perros** (mecánica de persecución/esquive), mayor densidad de baches
   - Es el mapa asociado naturalmente al modo de juego "perros"

5. **Avenida interprovincial — basada en "la Carretera del Norte"**
   - Ambiente: carretera abierta entre provincias, menos urbano, más velocidad
   - Obstáculos únicos: **vehículos rápidos en general** (autos a alta velocidad, camiones de carga en ruta) — la dificultad aquí viene más de la velocidad relativa que de la cantidad
   - Es el mapa más asociado al modo "contra reloj" o "gasolina" por su naturaleza de trayecto largo

### Notas para el arquitecto (pendiente de definir por el usuario)
- Aún no se ha decidido una mecánica adicional más allá del sistema día/tarde/noche + tráfico — el usuario indicó que puede surgir una idea nueva más adelante. No implementar mecánicas no listadas aquí sin confirmar primero.

## Fase actual del proyecto
**Fase 1: Prototipo jugable sin arte final.**
Objetivo: tener un ejecutable donde la moto se mueve, salta/esquiva obstáculos básicos, y se puede jugar al menos el modo "Contra reloj" de principio a fin con placeholders.

Nota: la Fase 1 usa UN solo mapa placeholder simple. El sistema de 5 mapas × 3 turnos se implementa en una fase posterior, una vez el prototipo base funcione.
