# Audio System Reference

Complete reference for Godot 4.6 audio: buses, streams, 3D audio, and patterns.

---

## Audio Bus Architecture

Audio flows through a **bus routing system**. Every sound passes through one or more buses before reaching speakers.

```
AudioStreamPlayer ──→ SFX Bus ──→ Master Bus ──→ Speakers
AudioStreamPlayer ──→ Music Bus ──→ Master Bus ──→ Speakers
AudioStreamPlayer3D ──→ Ambient Bus ──→ SFX Bus ──→ Master Bus ──→ Speakers
```

- **Master bus** (always leftmost) outputs to speakers. Signal must never exceed **0 dB** or clipping occurs.
- Non-master buses route **leftward** to other buses (prevents infinite loops).
- Buses auto-disable on silence (blue VU meter) to save CPU.
- Default layout saved at `res://default_bus_layout.tres`.

### Decibel Scale

| Value | Meaning |
|-------|---------|
| 0 dB | Maximum digital amplitude |
| -6 dB | Half amplitude |
| -12 dB | Quarter amplitude |
| -60 to -80 dB | Inaudible |

Every **6 dB** = doubling or halving of amplitude.

### Bus Effects

Add effects to buses (processed top to bottom):
- Reverb, Delay, Chorus, Phaser, Distortion
- EQ (10-band, 6-band, 21-band)
- Compressor, Limiter
- Low/High Pass Filter
- Amplify, Panner, Stereo Enhance

**GOTCHA (Web platform)**: Bus effects are unsupported when playback mode is "Sample" (default). Switch to "Stream" mode.

---

## Audio Player Nodes

| Node | Positional | Use Case |
|------|-----------|----------|
| `AudioStreamPlayer` | No | Music, UI sounds, global SFX |
| `AudioStreamPlayer2D` | 2D | Stereo panning by screen position |
| `AudioStreamPlayer3D` | 3D | Full spatial audio (stereo, 5.1, 7.1) |

### Basic Usage (C#)

```csharp
public partial class SoundManager : Node
{
    private AudioStreamPlayer _musicPlayer;

    public override void _Ready()
    {
        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
    }

    public void PlayMusic(AudioStream track, float fadeIn = 1.0f)
    {
        _musicPlayer.Stream = track;
        _musicPlayer.VolumeDb = -80f;
        _musicPlayer.Play();

        var tween = CreateTween();
        tween.TweenProperty(_musicPlayer, "volume_db", 0.0f, fadeIn);
    }

    public async void StopMusic(float fadeOut = 1.0f)
    {
        var tween = CreateTween();
        tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, fadeOut);
        await ToSignal(tween, Tween.SignalName.Finished);
        _musicPlayer.Stop();
    }
}
```

### 3D Audio (C#)

```csharp
public partial class AudioComponent : Node3D
{
    [Export] public AudioStream FootstepSound { get; set; }
    [Export] public AudioStream HitSound { get; set; }
    [Export] public AudioStream DeathSound { get; set; }

    private AudioStreamPlayer3D _sfxPlayer;
    private AudioStreamPlayer3D _voicePlayer;

    public override void _Ready()
    {
        _sfxPlayer = GetNode<AudioStreamPlayer3D>("SFXPlayer");
        _voicePlayer = GetNode<AudioStreamPlayer3D>("VoicePlayer");
    }

    public void PlayFootstep()
    {
        if (!_sfxPlayer.Playing)
        {
            _sfxPlayer.Stream = FootstepSound;
            _sfxPlayer.PitchScale = (float)GD.RandRange(0.9, 1.1); // Variation
            _sfxPlayer.Play();
        }
    }

    public void PlayHit()
    {
        _sfxPlayer.Stream = HitSound;
        _sfxPlayer.Play();
    }

    public void PlayDeath()
    {
        _voicePlayer.Stream = DeathSound;
        _voicePlayer.Play();
    }
}
```

### AudioStreamRandomizer

Selects from a list of streams with random pitch/volume shifts — great for footsteps, impacts, etc.:

1. Create `AudioStreamRandomizer` resource
2. Add multiple audio streams to it
3. Set random pitch and volume ranges
4. Assign to AudioStreamPlayer — it auto-selects a random stream each play

---

## 3D Audio Features

### Reverb Buses

Use `Area3D` nodes to divert audio to specific buses (e.g., different reverb per room):

```
Room1 (Area3D) → routes to "Cave Reverb" bus
Room2 (Area3D) → routes to "Hall Reverb" bus
```

Sends dry (direct) and wet (reverb) audio to separate buses. The `Uniformity` parameter simulates room reflection characteristics.

### Doppler Effect

Enable `Velocity Tracking` on AudioStreamPlayer3D:
- Set to **Idle** if movement in `_Process()`
- Set to **Physics** if movement in `_PhysicsProcess()`

Tracks velocity on both the player and the Camera3D.

---

## Audio Composition Pattern

```
Player (CharacterBody3D)
├── AudioComponent.tscn                    ← reusable component
│   ├── AudioStreamPlayer3D "SFX"         ← footsteps, impacts
│   ├── AudioStreamPlayer3D "Voice"       ← dialogue, grunts
│   └── AudioStreamPlayer3D "Weapon"      ← weapon sounds
├── HealthComponent.tscn
└── ...
```

Wire audio via signals:
```csharp
// In Player._Ready()
_health.DamageTaken += (amount, source) => _audio.PlayHit();
_health.Died += () => _audio.PlayDeath();
_movement.Moved += (vel) =>
{
    if (IsOnFloor() && vel.LengthSquared() > 0.1f)
        _audio.PlayFootstep();
};
```

---

## Recommended Bus Layout

```
Master (0 dB)
├── Music (-6 dB)
├── SFX (0 dB)
│   ├── Ambient (-3 dB)
│   └── UI (0 dB)
└── Voice (0 dB)
```

Each bus has its own volume slider in settings. Players can adjust Music, SFX, and Voice independently.

### Settings Menu Integration

```csharp
public void SetBusVolume(string busName, float linearValue)
{
    int busIndex = AudioServer.GetBusIndex(busName);
    AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(linearValue));
}

public void SetBusMuted(string busName, bool muted)
{
    int busIndex = AudioServer.GetBusIndex(busName);
    AudioServer.SetBusMute(busIndex, muted);
}

// linearValue: 0.0 = silent, 1.0 = full volume
// Store linear value in settings, convert to dB for AudioServer
```

**GOTCHA**: Stream players reference buses **by name**. Renaming a bus breaks all references.
