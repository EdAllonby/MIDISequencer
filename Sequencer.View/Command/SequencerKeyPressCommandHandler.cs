using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command
{
    /// <summary>
    /// Handles when a key is pressed.
    /// </summary>
    public sealed class SequencerKeyPressCommandHandler
    {
        [NotNull] private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForAllNotes;
        [NotNull] private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForSelectedNotes;

        [NotNull] private readonly ISequencerNotes notes;

        public SequencerKeyPressCommandHandler([NotNull] IMusicalSettings musicalSettings, [NotNull] ISequencerNotes notes, [NotNull] IKeyboardStateProcessor keyboardStateProcessor)
        {
            var ticksPerSixteenthNote = musicalSettings.TicksPerQuarterNote / 4;

            noteCommandsForSelectedNotes = new Dictionary<KeyboardInput, INotesCommand>
            {
                { new KeyboardInput(Key.Right), new MoveNotePositionCommand(ticksPerSixteenthNote) },
                { new KeyboardInput(Key.Add), new IncrementVelocityCommand(5) },
                { new KeyboardInput(Key.Subtract), new DecrementVelocityCommand(5) },
                { new KeyboardInput(Key.Left), new MoveNotePositionCommand(-ticksPerSixteenthNote) },
                { new KeyboardInput(Key.Up), new MoveNotePitchCommand(1) },
                { new KeyboardInput(Key.Down), new MoveNotePitchCommand(-1) },
                { new KeyboardInput(Key.Delete), new DeleteNotesCommand(notes) }
            };

            noteCommandsForAllNotes = new Dictionary<KeyboardInput, INotesCommand>
            {
                { new KeyboardInput(ModifierKeys.Control, Key.A), new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected) },
                { new KeyboardInput(Key.Escape), new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Unselected) }
            };

            this.notes = notes;
        }

        /// <summary>
        /// Handle's a particular key press from the user.
        /// </summary>
        /// <param name="keyPressed">The key the user pressed.</param>
        public void HandleSequencerKeyPressed(Key keyPressed)
        {
            var input = new KeyboardInput(Keyboard.Modifiers, keyPressed);

            Handle(input, noteCommandsForSelectedNotes, notes.SelectedNotes);
            Handle(input, noteCommandsForAllNotes, notes.AllNotes);
        }

        private static void Handle([NotNull] KeyboardInput input,
            [NotNull] IDictionary<KeyboardInput, INotesCommand> noteCommands, [NotNull] IEnumerable<IVisualNote> notesToExecute)
        {
            noteCommands.TryGetValue(input, out INotesCommand noteCommand);
            noteCommand?.Execute(notesToExecute);
        }
    }
}