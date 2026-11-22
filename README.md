# MIDISequencer

A piano-roll step sequencer written in C#. Draw notes on a one-measure grid and it loops them through a
built-in synthesiser, with a piano keyboard for trying out notes.

It started in 2015 as a WPF app for Windows. It now runs on .NET 10 with an [Avalonia](https://avaloniaui.net)
UI, so it runs natively on Apple Silicon Macs as well as Windows and Linux. The port is described in
[docs/macos-port-plan.md](docs/macos-port-plan.md).

Check the Issues page for things to do.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` pins 10.0.x)
- An audio output device

## Running

```sh
dotnet run --project Sequencer.View
```

In VS Code (with the C# Dev Kit extension), press **F5**. The **Sequencer** launch configuration builds
and runs the app.

### macOS app bundle

```sh
scripts/bundle-macos.sh            # Apple Silicon
scripts/bundle-macos.sh osx-x64    # Intel
open dist/Sequencer.app
```

This builds a self-contained, ad-hoc-signed `dist/Sequencer.app` that you can drag into Applications.
It runs on the Mac that built it. Giving it to other people would need a Developer ID signature and
notarisation.

## Tests

```sh
dotnet test
```

`Sequencer.View.Tests` includes UI tests that drive the real main window with Avalonia's headless
platform, so they need no display. `Window_RendersTheEditor` saves a screenshot to the test output folder.

## Using the sequencer

On macOS, ⌘ is the command key; on Windows and Linux it is Ctrl.

| Action | Mouse / keys |
|---|---|
| Switch tool (Create, Select, Delete) | Hold right-click (or two-finger click, or Ctrl-click on macOS), move towards a slice of the radial menu, release |
| Create a note | Create tool: click; drag to lengthen |
| Select notes | Select tool: click, drag a box, or ⌘-click to add or remove |
| Move selected notes | Select tool: drag a note (hold Shift to keep the pitch), or use the arrow keys |
| Resize a note | Select tool: drag its left or right edge |
| Delete notes | Delete tool: click; or select and press Delete / Backspace |
| Velocity | `+`/`=` and `-` (main keyboard or numeric keypad) |
| Select all / deselect | ⌘A / Esc |
| Play / pause | Space |
| Stop | Return |
| Tempo | Type in the BPM box (30-300), or click **Tap** in time |
| Play a note | Press a key on the piano keyboard; drag across keys to play each in turn |

Keys on the piano keyboard light up as their notes play back. Creating a note, or changing its pitch,
plays it briefly. Switch this off with **Play notes while editing**.

The panel on the right shows the selected note's pitch, velocity, start and end, and the **Synth**
settings: waveform (sawtooth, square, triangle or sine), attack, decay, sustain, release and volume.
Volume changes apply immediately; the others apply to the next note played.

## Projects

| Project | Contents |
|---|---|
| `Sequencer.Domain` | Pitches, positions, velocities, tones and settings |
| `Sequencer.Midi` | The tempo clock (a `Stopwatch`-timed thread), tick/position maths, MIDI note numbers |
| `Sequencer.Audio` | Voices with ADSR envelopes, wavetable waveforms, previews, one mixed stream played through PortAudio |
| `Sequencer.ViewModel` | The main view model (CommunityToolkit.Mvvm) |
| `Sequencer.Visual` | Notes on the editor canvas, and input state |
| `Sequencer.View` | The Avalonia app: window, editor controls, radial menu, and Autofac set-up |
| `Sequencer.Utilities` | Small helpers |

Each project has a matching `*.Tests` project (NUnit 4 and Moq). Package versions are set centrally in
`Directory.Packages.props`.
