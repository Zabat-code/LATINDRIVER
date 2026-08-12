using Godot;

// HUD.cs
// HUD mínimo del prototipo Fase 1: muestra distancia recorrida, tiempo restante
// del modo y un estado (Jugando / Game Over / ¡Meta!). Usa un Label hijo
// definido en HUD.tscn.
// En fases posteriores se reemplaza por fuentes/estilo definitivo
// (ver PROJECT_CONTEXT.md).
//
// Nota de implementación: cada setter actualiza SOLO su línea dentro de un
// text de 3 líneas (índices 0=distancia, 1=tiempo, 2=estado) para no pisar
// la información de los otros setters.

public partial class HUD : Control
{
	private Label _label;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
		// Estado inicial: 3 líneas separadas.
		_label.Text = "Distancia: 0 m\nTiempo: 0.0s\nJugando";
	}

	public void SetDistance(float x)
	{
		var lines = _label.Text.Split('\n');
		if (lines.Length < 3) lines = new string[] { "Distancia: 0 m", "Tiempo: 0.0s", "Jugando" };
		lines[0] = $"Distancia: {x / 32f:F0} m";
		_label.Text = string.Join("\n", lines);
	}

	public void SetTime(float t)
	{
		var lines = _label.Text.Split('\n');
		if (lines.Length < 3) lines = new string[] { "Distancia: 0 m", "Tiempo: 0.0s", "Jugando" };
		lines[1] = $"Tiempo: {t:F1}s";
		_label.Text = string.Join("\n", lines);
	}

	public void SetStatus(string status)
	{
		var lines = _label.Text.Split('\n');
		if (lines.Length < 3) lines = new string[] { "Distancia: 0 m", "Tiempo: 0.0s", "Jugando" };
		lines[2] = status;
		_label.Text = string.Join("\n", lines);
	}
}
