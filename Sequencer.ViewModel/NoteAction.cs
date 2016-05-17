namespace Sequencer.ViewModel
{
    /// <summary>
    /// Actions a note can take.
    /// </summary>
    public enum NoteAction
    {
        /// <summary>
        /// Allow to select and reposition a note.
        /// </summary>
        Select,

        /// <summary>
        /// Allow to create a new note.
        /// </summary>
        Create,

        /// <summary>
        /// Allow to delete a note.
        /// </summary>
        Delete
    }
}