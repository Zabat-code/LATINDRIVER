using Godot;
using System;

// MapConfig.cs
// Definición de los 5 mapas y 3 turnos (Día/Tarde/Noche) del plan de LATINDRIVER.
// Un mapa es el mismo layout físico; el turno ajusta densidad de tráfico,
// velocidad de obstáculos y paleta de color (ver PROJECT_CONTEXT.md).
// El menú selecciona MapIndex + TurnIndex; GameManager los lee al arrancar Level.

public enum MapType
{
    Pueblito,       // Nagua: animales sueltos, tráfico bajo
    CiudadColonial, // Zona Colonial SD: carros + furgonetas
    Megaciudad,     // Gazcue: jeepetas + camiones, alta densidad
    CiudadMarginada,// perros + baches, asociado a modo Chase
    AvenidaInterprovincial // Carretera del Norte: velocidad relativa
}

public enum TurnType { Dia, Tarde, Noche }

public static class MapConfig
{
    // Datos base por mapa (sin turno).
    public struct MapData
    {
        public string Nombre;
        public float DensidadBase;     // multiplicador de frecuencia de spawn
        public float VelocidadBase;    // multiplicador de velocidad de obstáculos
        public Color TinteDia;
        public Color TinteTarde;
        public Color TinteNoche;
    }

    public static MapData[] Maps = new MapData[]
    {
        new MapData { Nombre = "Pueblito (Nagua)", DensidadBase = 0.7f, VelocidadBase = 0.85f,
            TinteDia = new Color(0.95f,0.92f,0.82f), TinteTarde = new Color(1.0f,0.85f,0.6f), TinteNoche = new Color(0.25f,0.28f,0.4f) },
        new MapData { Nombre = "Ciudad Colonial (SD)", DensidadBase = 1.0f, VelocidadBase = 1.0f,
            TinteDia = new Color(0.95f,0.9f,0.78f), TinteTarde = new Color(1.0f,0.8f,0.55f), TinteNoche = new Color(0.3f,0.26f,0.35f) },
        new MapData { Nombre = "Megaciudad (Gazcue)", DensidadBase = 1.4f, VelocidadBase = 1.15f,
            TinteDia = new Color(0.9f,0.9f,0.88f), TinteTarde = new Color(1.0f,0.82f,0.62f), TinteNoche = new Color(0.2f,0.22f,0.32f) },
        new MapData { Nombre = "Ciudad Marginada", DensidadBase = 1.2f, VelocidadBase = 0.95f,
            TinteDia = new Color(0.85f,0.82f,0.75f), TinteTarde = new Color(0.95f,0.75f,0.5f), TinteNoche = new Color(0.18f,0.18f,0.25f) },
        new MapData { Nombre = "Avenida Interprovincial", DensidadBase = 1.0f, VelocidadBase = 1.3f,
            TinteDia = new Color(0.92f,0.95f,0.9f), TinteTarde = new Color(1.0f,0.88f,0.7f), TinteNoche = new Color(0.22f,0.25f,0.38f) },
    };

    // Multiplicadores por turno (Día fácil -> Noche difícil).
    public static float TurnDensidad(TurnType t) => t == TurnType.Dia ? 0.8f : (t == TurnType.Tarde ? 1.0f : 1.35f);
    public static float TurnVelocidad(TurnType t) => t == TurnType.Dia ? 0.9f : (t == TurnType.Tarde ? 1.0f : 1.25f);

    public static Color TinteMapa(int mapIndex, TurnType turn)
    {
        var m = Maps[Mathf.Clamp(mapIndex, 0, Maps.Length - 1)];
        return turn == TurnType.Dia ? m.TinteDia : (turn == TurnType.Tarde ? m.TinteTarde : m.TinteNoche);
    }
}
