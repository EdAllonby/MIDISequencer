using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Command
{
    /// <summary>
    /// Handles when a key is pressed.
    /// </summary>
    public sealed class SequencerKeyPressCommandHandler
    {
        private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForAllNotes;
        private readonly IDictionary<KeyboardInput, INotesCommand> noteCommandsForSelectedNotes;

        private readonly ISequencerNotes notes;

        public SequencerKeyPressCommandHandler([NotNull] ISequencerNotes notes, IKeyboardStateProcessor keyboardStateProcessor)
        {
            noteCommandsForSelectedNotes = new Dictionary<KeyboardInput, INotesCommand>
            {
                { new KeyboardInput(Key.A), new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected) },
                { new KeyboardInput(Key.Right), new MoveNotePositionCommand(1) },
                { new KeyboardInput(Key.Add), new IncrementVelocityCommand(5) },
                { new KeyboardInput(Key.Subtract), new DecrementVelocityCommand(5) },
                { new KeyboardInput(Key.Left), new MoveNotePositionCommand(-1) },
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

        private static void Handle(KeyboardInput input, IDictionary<KeyboardInput, INotesCommand> noteCommands, IEnumerable<IVisualNote> notesToExecute)
        {
            INotesCommand noteCommand;
            noteCommands.TryGetValue(input, out noteCommand);
            noteCommand?.Execute(notesToExecute);
        }
    }
}