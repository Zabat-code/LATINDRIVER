# LATINDRIVER — Recomendaciones de Arquitectura y Arte

**Fecha:** 2026-10-06
**Fuente:** gpt-6.1-sol (consultoría de arquitectura y arte)

---

## 🏗️ Recomendaciones de Arquitectura

### 1. Problemas identificados

| Problema | Impacto |
|---|---|
| `Dog.cs` es `Node2D` sin collider | Atraviesa obstáculos, no hay interacción |
| `Player.cs` avanza + obstáculos retroceden | Velocidad de encuentro duplicada |
| `GameManager.cs` instancia GameOver en raíz | El nivel sigue activo, pantalla sobrevive al reinicio |

### 2. Distribución de responsabilidades

| Componente | Responsabilidad |
|---|---|
| `GameManager` | Estados, inicio y final de partida |
| `IGameMode` | Reglas: tiempo, captura, combustible, dificultad |
| `LevelGenerator` | Elegir patrones y solicitar objetos al pool |
| `Player` / `Dog` | Movimiento y respuesta a impactos |
| `SceneRouter` (autoload) | Navegación entre escenas |
| `RunSession` (autoload) | Selección actual y último resultado |

### 3. Modelo de movimiento

**Recomendación:** Mantener al jugador fijo en X, desplazar contenido hacia la izquierda.

```csharp
DistanceTraveled += ScrollSpeed * (float)delta;
```

- Cámara, carretera, obstáculos y combustible comparten `ScrollSpeed`
- Perros tienen velocidad relativa de acercamiento independiente
- Separar **posición sobre la carretera** de **altura del salto**

### 4. Capas de colisión

| Área | Capa | Máscara que detecta |
|---|---:|---|
| Jugador | 1 | Obstáculos 2, perros 3, combustible 4 |
| Obstáculo | 2 | Ninguna |
| Perro | 3 | Obstáculos 2 |
| Combustible | 4 | Ninguna |
| Proyectil | 5 | Perros 3 |

### 5. Mecánica de perros vs obstáculos

1. El perro persigue la **posición de suelo** del jugador
2. Reacciona al cambio de carril con retraso de **0.25-0.40s**
3. Al tocar un obstáculo pasa de `Chasing` a `Stunned`
4. Durante **0.6-1s** deja de acercarse y pierde terreno con el scroll
5. Recupera la persecución o se retira si queda fuera de pantalla

### 6. Pool de objetos

```csharp
public interface IPoolable
{
    void Activate(Vector2 position, float speed);
    void Deactivate();
}
```

- Un pool por escena: conos, barriles, vehículos, perros, combustible, proyectiles
- Capacidad: `frecuencia máxima × tiempo en pantalla + 25% margen`
- Ejemplo: 2 objetos/s × 8s + 25% ≈ **20 instancias**
- Limitar jauría a **3-5 perros**

### 7. Transiciones de escena

1. Cambiar a estado `Ending` (una sola vez)
2. Detener spawn, input y simulación
3. Guardar `RunResult`: modo, mapa, distancia, tiempo, causa de derrota
4. Ejecutar fundido
5. Cambiar a `GameOver.tscn`

---

## 🎨 Recomendaciones de Arte Pixel Art

### 1. Paleta tropical (16 colores)

| Función | Colores |
|---|---|
| Contorno y sombras | `#18243A`, `#303B59` |
| Carretera y metal | `#596579`, `#9AA6AD` |
| Crema y arena | `#FFF0CC`, `#DDB878` |
| Coral y peligro | `#ED705F`, `#A83E50` |
| Naranja y amarillo | `#F49A45`, `#F5D76A` |
| Vegetación | `#245B48`, `#438C59`, `#91C66C` |
| Mar y cielo | `#247D91`, `#45BDB4`, `#8BD4E8` |

### 2. Diseño de sprites

| Asset | Diseño y animación |
|---|---|
| Motocicleta + conductor | Ruedas legibles, casco distintivo; 4 frames marcha, 2 inclinación |
| Perro | Hocico, cola y patas separados; 6 frames carrera, 2-3 tropiezo |
| Cono | Naranja con franja crema; estático |
| Barril | Aros metálicos y tapa visible; estático o 4 frames |
| Carro | Perfil bajo, techo claramente definido |
| Camioneta | Cabina y cajón separados visualmente |
| Jeepeta | Techo alto, ruedas grandes, carrocería vertical |
| Bidón | Asa y tapa visibles; 4 frames de brillo |
| Animal | Gallina o chivo; silueta distinta del perro |

### 3. Fondos y progresión visual

| Mapa | Identidad visual | Progresión |
|---|---|---|
| Zona colonial | Piedra crema, fachadas coral, balcones | Espacios amplios, pocos detalles |
| Barrio | Colmados, zinc, cables, colores variados | Más densidad fuera de carretera |
| Avenida | Medianas, postes, señales, edificios | Patrones de obstáculos exigentes |
| Ciudad poblada | Fachadas superpuestas, letreros, ventanas | Fondo denso, acentos nocturnos |

### 4. Resolución

- **Decidir:** 1280×720 (actual) vs 1152×648 (fondos generados)
- **Recomendación:** Usar 1152×648 como resolución lógica
- **Alternativa retro:** Dibujar a 576×324 y escalar ×2 (sprite 32×32 = 64×64)

### 5. Dificultad visual

- Tiempo de reacción = distancia visible / velocidad de encuentro
- Inicio: **1.5s** de reacción
- Fases difíciles: **0.9-1.1s** de reacción
- Asegurar que siempre haya un carril vacío alcanzable

---

## 📋 Plan de implementación

1. **Unificar scroll** y posiciones de suelo
2. **Implementar perro vs obstáculo** (collider + estado Stunned)
3. **Ordenar transiciones** (SceneRouter autoload)
4. **Introducir pooling** (Pool de objetos)
5. **Producir sección colonial** (moto, perro, cono) para validar escala
6. **Generar catálogo completo** de sprites y fondos
7. **Generar música** con midi-orquesta (Mega Man + tropical)

---

**Última actualización:** 2026-10-06
