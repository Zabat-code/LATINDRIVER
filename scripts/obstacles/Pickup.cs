using Godot;

// Pickup.cs
// Ítem recolectable del modo "Gasolina" (FuelMode): bidones de gasolina.
// Es un StaticBody2D en grupo "pickup"; el Player detecta el grupo y emite
// PlayerCollected(itemType), que GameManager enruta a
// IGameMode.OnPlayerCollectItem. No está en "obstacle", así que no termina la
// partida. Placeholder; en Fase 2 se reemplaza por pixel art.

public partial class Pickup : StaticBody2D
{
	[Export] public string ItemType = "fuel";

	public string GetItemType()
	{
		return ItemType;
	}

	public override void _Ready()
	{
		AddToGroup("pickup");
	}
}
