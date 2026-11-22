using System.Collections.Generic;
using Avalonia.Input;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command
{
    /// <summary>
    /// Maps keyboard shortcuts to note commands. Shortcuts follow the platform: ⌘ on macOS and Ctrl
    /// elsewhere, and keys that a laptop keyboard lacks (forward Delete, the numeric keypad) have
    /// main-keyboard alternatives.
    /// </summary>
    public sealed class SequencerKeyPressCommandHandler
    {
        [NotNull] private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForAllNotes;
        [NotNull] private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForSelectedNotes;
        [NotNull] private readonly ISequencerNotes notes;

        public SequencerKeyPressCommandHandler([NotNull] IMusicalSettings musicalSettings, [NotNull] ISequencerNotes notes,
            [NotNull] IKeyboardStateProcessor keyboardStateProcessor, KeyModifiers commandModifier)
        {
            int ticksPerSixteenthNote = musicalSettings.TicksPerQuarterNote / 4;
            var deleteNotes = new DeleteNotesCommand(notes);
            var incrementVelocity = new IncrementVelocityCommand(5);
            var decrementVelocity = new DecrementVelocityCommand(5);

            noteCommandsForSelectedNotes = new Dictionary<KeyboardInput, INotesCommand>
            {
                { new KeyboardInput(Key.Right), new MoveNotePositionCommand(ticksPerSixteenthNote) },
                { new KeyboardInput(Key.Left), new MoveNotePositionCommand(-ticksPerSixteenthNote) },
                { new KeyboardInput(Key.Up), new MoveNotePitchCommand(1) },
                { new KeyboardInput(Key.Down), new MoveNotePitchCommand(-1) },

                // Numeric keypad, plus the =/+ and - keys on the main keyboard (with or without Shift).
                { new KeyboardInput(Key.Add), incrementVelocity },
                { new KeyboardInput(Key.OemPlus), incrementVelocity },
                { new KeyboardInput(KeyModifiers.Shift, Key.OemPlus), incrementVelocity },
                { new KeyboardInput(Key.Subtract), decrementVelocity },
                { new KeyboardInput(Key.OemMinus), decrementVelocity },

                // A Mac keyboard's "delete" key sends Backspace; fn+delete sends Delete.
                { new KeyboardInput(Key.Delete), deleteNotes },
                { new KeyboardInput(Key.Back), deleteNotes }
            };

            noteCommandsForAllNotes = new Dictionary<KeyboardInput, INotesCommand>
            {
                { new KeyboardInput(commandModifier, Key.A), new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected) },
                { new KeyboardInput(Key.Escape), new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Unselected) }
            };

            this.notes = notes;
        }

        /// <summary>
        /// Runs the command for this key, if there is one. Returns whether a command matched.
        /// </summary>
        public bool HandleSequencerKeyPressed(Key keyPressed, KeyModifiers modifiers)
        {
            var input = new KeyboardInput(modifiers, keyPressed);
            bool handled = Handle(input, noteCommandsForSelectedNotes, notes.SelectedNotes);
            handled |= Handle(input, noteCommandsForAllNotes, notes.AllNotes);
            return handled;
        }

        private static bool Handle([NotNull] KeyboardInput input,
            [NotNull] IDictionary<KeyboardInput, INotesCommand> noteCommands, [NotNull] IEnumerable<IVisualNote> notesToExecute)
        {
            if (!noteCommands.TryGetValue(input, out INotesCommand noteCommand))
            {
                return false;
            }

            noteCommand.Execute(notesToExecute);
            return true;
        }
    }
}
