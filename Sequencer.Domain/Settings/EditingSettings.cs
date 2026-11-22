namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// Editor preferences, shared by every track.
    /// </summary>
    public sealed class EditingSettings
    {
        /// <summary>
        /// Play notes and drum hits as they are added or changed.
        /// </summary>
        public bool PreviewOnEdit { get; set; } = true;
    }
}
