using Godot;

// MusicManager.cs
// Reproduce la musica del nivel (OGG/WAV en res://assets/audio/music/) en bucle
// y expone control de volumen. Godot NO reproduce MIDI; si queres usar los .mid
// del disco C:, convertilos a .ogg (p.ej. con fluidsynth) y dejalos en esa carpeta.
public partial class MusicManager : Node
{
	private AudioStreamPlayer _player;

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		_player.Bus = "Master";
		AddChild(_player);

		var stream = GD.Load<AudioStream>("res://assets/audio/music/loop1.ogg");
		if (stream != null)
		{
			_player.Stream = stream;
			_player.StreamPaused = false;
			// bucle: los AudioStream de Godot repiten por defecto si Loop=true
			var loopable = _player.Stream as AudioStream;
			_player.Play();
		}
		else
		{
			GD.Print("[MUSIC] no se encontro loop1.ogg");
		}
	}

	// volumen lineal 0..1 -> dB (0 = silencio, 1 = max)
	public void SetVolume(float linear)
	{
		linear = Mathf.Clamp(linear, 0f, 1f);
		if (_player != null)
			_player.VolumeDb = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
	}

	public void Stop()
	{
		if (_player != null && _player.Playing) _player.Stop();
	}
}
