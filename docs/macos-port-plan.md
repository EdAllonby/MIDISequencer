# macOS port plan

Goal: run the sequencer natively on Apple Silicon macOS (and keep Windows/Linux) by replacing WPF with Avalonia UI.

## Decisions

| Area | Decision |
|---|---|
| UI framework | Avalonia UI on .NET 10 (LTS). Clean switch: the WPF View is retired, git history keeps it. |
| Scope | Port, and fix what we touch. No new features. |
| MVVM | MvvmLight → CommunityToolkit.Mvvm. Remove CommonServiceLocator; inject dependencies into controls instead of calling `ServiceLocator.Current`. |
| DI / logging | Keep Autofac and log4net, upgraded to current versions. Log to the terminal instead of the Win32 console (`UnsafeNativeMethods` is deleted). |
| Tests | NUnit and Moq upgraded. Remove the dead CodedUI references from `Sequencer.View.Tests` and keep its command tests. |
| Audio | Keep NAudio.Core for sample providers and the envelope. Mix all voices into one `MixingSampleProvider`, sent to the speakers through an `IAudioOutput` interface backed by a cross-platform library (picked after a test app proves it on osx-arm64). Note-off gates the envelope instead of stopping the device; also fix the per-second phase reset. This fixes #15 and #16. |
| Clock | A new `IInternalClock` running on a dedicated `Stopwatch` thread replaces Sanford's `MidiInternalClock`. Tick-driven UI updates are passed to the UI thread with `Dispatcher.UIThread.Post`. Fix the `-=`/`+=` bug in the `SequencerClock` `Started`/`Stopped` event remove accessors. |
| WPF types below the View | `Visual` moves to Avalonia types (`Canvas`, `Shapes`, `Control`, `Key`, `KeyModifiers`, `Point`, `Geometry`). `Domain` and `Utilities` use `System.Drawing.Color`/`PointF` and have no UI dependency. |
| Property grid | Replaced by a hand-built note inspector bound to a `SelectedNoteViewModel`. |
| Keyboard | Platform-aware: ⌘ on macOS, Ctrl elsewhere. Backspace and Delete both delete notes. `=`/`-` work alongside numpad `+`/`-`. Check modifier flags instead of `Key.LeftCtrl`/`Key.LeftShift`. |
| Tooling | VS Code (`.vscode/launch.json`, `tasks.json`). |
| Distribution | `scripts/bundle-macos.sh` produces an ad-hoc-signed `Sequencer.app` for osx-arm64. |

## Milestones

All work happens on the `macos-port` branch. Each milestone is a commit with passing tests on macOS, and the branch merges to `master` in one PR once the app runs and plays on the Mac.

1. **SDK-style projects.** Convert every project to SDK-style `net10.0` with `PackageReference`. The non-UI libraries and their tests build and pass on macOS; the WPF View is temporarily left out of the solution.
2. **Remove WPF from the core.** Replace the WPF types in `Domain`, `Utilities` and `ViewModel`, switch to CommunityToolkit.Mvvm, and remove the service locator.
3. **Audio and clock.** Build a test app for the output library on osx-arm64, then the mixer behind `IAudioOutput`, then the `Stopwatch` clock.
4. **Avalonia UI.** Move `Visual` to Avalonia, build the new Avalonia View (main window, sequencer control, header, drag-selection box, radial context menu, note inspector), and add platform-aware key mapping.
5. **Retire WPF.** Delete the WPF View code, check everything works as it did on Windows, and update the README.
6. **App bundle.** Add the `.app` bundle script.

## Parity checklist

- [ ] Create, move, resize and delete notes with the mouse
- [ ] Drag-box selection; ⌘-click adds to the selection
- [ ] Radial context menu (right-click / two-finger click) switches the note action
- [ ] Keyboard: arrows move position and pitch, velocity up and down, delete, select all, Escape deselects
- [ ] Play, pause and stop (Space / Enter), looping at the end of the sequence
- [ ] Position indicator in the header and on the grid
- [ ] Note inspector edits the selected note
- [ ] Notes play with attack and release, without clicks
