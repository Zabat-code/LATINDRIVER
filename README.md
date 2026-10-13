# LATIN / DRIVER

> **LATINDRIVER v0.6 — a four-lane 2D road runner set on Dominican routes.**

LATINDRIVER is a pixel-art-inspired arcade driving game built with Godot 4.7.1 and C#. Ride through tropical roads, choose a route, select a challenge, and survive traffic, animals, potholes, and street hazards.

## Latest build

The latest playable artifact found in the workspace is **v0.6 for Linux x86_64**. It was launched under Xvfb with Godot's OpenGL compatibility renderer and remained running for the verification window without startup or runtime errors.

The current `main` source includes the updated directional controls: **Left arrow = throw backward** and **Right arrow = throw forward**. The v0.6 binary predates this source change because the Godot .NET export template is not installed in this environment; rebuild the project with Godot 4.7.1 .NET to obtain a binary containing the new mapping.

## Features

- Four-lane road movement with keyboard controls.
- Dominican-inspired routes, starting with **Nagua · Brisa del Atlántico**.
- Challenge selection: delivery / time attack, chase, fuel, and free driving.
- Easy, normal, and intense difficulty profiles.
- Obstacles and hazards: cars, animals, potholes, cones, and crates.
- Throwing mechanics, nitro, pause, retry, and mute controls.
- Original pixel-art direction and MIDI-driven music.
- Godot 4.7.1 .NET / C# project.

## Controls

| Action | Keys |
|---|---|
| Change lane | A / D |
| Throw backward | Left arrow or X |
| Throw forward | Right arrow |
| Jump / crouch | Up / Down arrows |
| Nitro | Space |
| Pause | P / Esc |
| Retry | R |
| Mute | M |

## Build from source

Requirements:

- Godot 4.7.1 .NET
- .NET 8 SDK

```bash
dotnet build LATINDRIVER.csproj --nologo
```

The source project is the canonical development track. The v0.6 Linux build is distributed through the GitHub release attached to this repository.

## Presentation

The portfolio presentation is available in [`docs/index.html`](docs/index.html). It uses the game's own visual language, route artwork, menu capture, controls, and feature set rather than a generic landing-page template.

## Status

**Playable prototype / portfolio build.** The core driving loop is implemented; maps, art, balance, and production packaging can continue to evolve.

## License

See the repository contents and asset documentation before redistributing the build or individual assets.
