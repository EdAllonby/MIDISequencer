using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command;
using Sequencer.Domain;

namespace Sequencer
{
    /// <summary>
    /// Handles when a key is pressed.
    /// </summary>
    public sealed class SequencerKeyPressHandler
    {
        private readonly SequencerNotes notes;

        private readonly IDictionary<Key, INotesCommand> noteCommandsForSelectedNotes;
        private readonly UpdateNoteStateCommand selectNotesCommand;
        public SequencerKeyPressHandler([NotNull] SequencerNotes notes)
        {
            selectNotesCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);

            noteCommandsForSelectedNotes = new Dictionary<Key, INotesCommand>
            {
                {Key.A, new UpdateNoteStateCommand(notes, NoteState.Selected)},
                {Key.Right, new MoveNotePositionCommand(1)},
                {Key.Left, new MoveNotePositionCommand(-1)},
                {Key.Up, new MoveNotePitchCommand(1)},
                {Key.Down, new MoveNotePitchCommand(-1)},
                {Key.Delete, new DeleteNotesCommand(notes)}
            };

            this.notes = notes;
        }

        /// <summary>
        /// Handle's a particular key press from the user.
        /// </summary>
        /// <param name="keyPressed">The key the user pressed.</param>
        public void HandleSequencerKeyPressed(Key keyPressed)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (keyPressed == Key.A))
            {
                selectNotesCommand.Execute(notes.All);
                return;
            }

            INotesCommand noteCommand;
            noteCommandsForSelectedNotes.TryGetValue(keyPressed, out noteCommand);
            noteCommand?.Execute(notes.SelectedNotes);
        }
    }
}