using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Command;
using Sequencer.Domain;

namespace Sequencer
{
    /// <summary>
    /// Handles when a key is pressed.
    /// </summary>
    public sealed class SequencerKeyPressHandler
    {
        private readonly IEnumerable<VisualNote> notes;

        private readonly MoveNotePitchCommand moveNoteDownCommand;
        private readonly MoveNotePositionCommand moveNoteLeftCommand;
        private readonly MoveNotePositionCommand moveNoteRightCommand;
        private readonly MoveNotePitchCommand moveNoteUpCommand;
        private readonly DeleteNotesCommand deleteNotesCommand;
        private readonly UpdateNoteStateCommand selectNoteCommand;

        public SequencerKeyPressHandler([NotNull] List<VisualNote> notes)
        {
            this.notes = notes;

            moveNoteLeftCommand = new MoveNotePositionCommand(-1);
            moveNoteRightCommand = new MoveNotePositionCommand(1);
            moveNoteUpCommand = new MoveNotePitchCommand(1);
            moveNoteDownCommand = new MoveNotePitchCommand(-1);
            deleteNotesCommand = new DeleteNotesCommand(notes);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
        }

        private IEnumerable<VisualNote> SelectedNotes
        {
            get { return notes.Where(x => x.NoteState == NoteState.Selected); }
        }

        /// <summary>
        /// Handle's a particular key press from the user.
        /// </summary>
        /// <param name="keyPressed">The key the user pressed.</param>
        public void HandleSequencerKeyPressed(Key keyPressed)
        {
            if (keyPressed == Key.Delete)
            {
                deleteNotesCommand.Execute(SelectedNotes);
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && (keyPressed == Key.A))
            {
                selectNoteCommand.Execute(notes);
            }
            if (keyPressed == Key.Left)
            {
                moveNoteLeftCommand.Execute(SelectedNotes);
            }
            if (keyPressed == Key.Right)
            {
                moveNoteRightCommand.Execute(SelectedNotes);
            }
            if (keyPressed == Key.Up)
            {
                moveNoteUpCommand.Execute(SelectedNotes);
            }
            if (keyPressed == Key.Down)
            {
                moveNoteDownCommand.Execute(SelectedNotes);
            }
        }
    }
}
