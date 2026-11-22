using System;
using JetBrains.Annotations;

namespace Sequencer.Domain.Arrangement
{
    public enum PlaybackMode
    {
        /// <summary>
        /// Loop one section, to work on it.
        /// </summary>
        Section,

        /// <summary>
        /// Play every section in order, to hear the song.
        /// </summary>
        Song
    }

    /// <summary>
    /// What is being edited and how playback runs. The UI thread writes it; playback reads it.
    /// </summary>
    public sealed class ArrangementState
    {
        private volatile int mode;
        private volatile int loopSection;
        [CanBeNull] private volatile Track selectedTrack;
        private volatile int selectedSection;

        public event EventHandler Changed;

        public PlaybackMode Mode
        {
            get => (PlaybackMode) mode;
            set
            {
                mode = (int) value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The section that loops in <see cref="PlaybackMode.Section"/> mode.
        /// </summary>
        public int LoopSection
        {
            get => loopSection;
            set
            {
                loopSection = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        [CanBeNull]
        public Track SelectedTrack
        {
            get => selectedTrack;
            set
            {
                selectedTrack = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SelectedSection
        {
            get => selectedSection;
            set
            {
                selectedSection = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
