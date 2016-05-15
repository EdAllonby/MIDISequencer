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
        private readonly DeleteNotesCommand deleteNotesCommand;

        private readonly MoveNotePitchCommand moveNoteDownCommand;
        private readonly MoveNotePositionCommand moveNoteLeftCommand;
        private readonly MoveNotePositionCommand moveNoteRightCommand;
        private readonly MoveNotePitchCommand moveNoteUpCommand;
        private readonly SequencerNotes notes;
        private readonly UpdateNoteStateCommand selectNoteCommand;

        public SequencerKeyPressHandler([NotNull] SequencerNotes notes)
        {
            this.notes = notes;

            moveNoteLeftCommand = new MoveNotePositionCommand(-1);
            moveNoteRightCommand = new MoveNotePositionCommand(1);
            moveNoteUpCommand = new MoveNotePitchCommand(1);
            moveNoteDownCommand = new MoveNotePitchCommand(-1);
            deleteNotesCommand = new DeleteNotesCommand(notes);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
        }

        /// <summary>
        /// Handle's a particular key press from the user.
        /// </summary>
        /// <param name="keyPressed">The key the user pressed.</param>
        public void HandleSequencerKeyPressed(Key keyPressed)
        {
            if (keyPressed == Key.Delete)
            {
                deleteNotesCommand.Execute(notes.SelectedNotes);
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && (keyPressed == Key.A))
            {
                selectNoteCommand.Execute(notes.All);
            }
            if (keyPressed == Key.Left)
            {
                moveNoteLeftCommand.Execute(notes.SelectedNotes);
            }
            if (keyPressed == Key.Right)
            {
                moveNoteRightCommand.Execute(notes.SelectedNotes);
            }
            if (keyPressed == Key.Up)
            {
                moveNoteUpCommand.Execute(notes.SelectedNotes);
            }
            if (keyPressed == Key.Down)
            {
                moveNoteDownCommand.Execute(notes.SelectedNotes);
            }
        }
    }
}