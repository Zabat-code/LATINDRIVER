using Godot;

// HUD.cs
// HUD minimo del prototipo: muestra distancia recorrida, tiempo restante del
// modo y un estado (Jugando / Game Over / Meta!).
// En fases posteriores se reemplaza por fuentes/estilo definitivo (ver
// PROJECT_CONTEXT.md).
//
// Implementacion con 3 Labels independientes (Distancia / Tiempo / Estado) en
// vez de un unico Label multilinea: cada setter actualiza SOLO su propio
// Label, asi el nombre del modo (Estado) persiste y nunca se pisa al
// refrescar distancia/tiempo. (Bug corregido: el Label multilinea quedaba
// recortado y el nombre del modo se ocultaba en pantalla.)

public partial class HUD : Control
{
private Label _distanceLabel;
private Label _timeLabel;
private Label _statusLabel;

public override void _Ready()
{
// NodeUtils por robustez (el build embebido 4.7.1 falla a veces con
// GetNode por nombre; ver NodeUtils.cs).
_distanceLabel = NodeUtils.GetChildByName<Label>(this, "DistanceLabel");
_timeLabel     = NodeUtils.GetChildByName<Label>(this, "TimeLabel");
_statusLabel   = NodeUtils.GetChildByName<Label>(this, "StatusLabel");

if (_distanceLabel != null) _distanceLabel.Text = "Distancia: 0 m";
if (_timeLabel    != null) _timeLabel.Text     = "Tiempo: 0.0s";
if (_statusLabel  != null) _statusLabel.Text   = "Jugando";
}

public void SetDistance(float x)
{
if (_distanceLabel != null) _distanceLabel.Text = $"Distancia: {x / 32f:F0} m";
}

public void SetTime(float t)
{
if (_timeLabel != null) _timeLabel.Text = $"Tiempo: {t:F1}s";
}

public void SetStatus(string status)
{
if (_statusLabel != null) _statusLabel.Text = status;
}
}
