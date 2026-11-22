using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// A read-only summary of the selected note, for the inspector panel.
    /// </summary>
    public sealed class SelectedNoteViewModel
    {
        public SelectedNoteViewModel([NotNull] Tone tone)
        {
            Pitch = tone.Pitch.ToString();
            Velocity = tone.Velocity.Value;
            Start = tone.StartPosition.ToString();
            End = tone.EndPosition.ToString();
        }

        [NotNull]
        public string Pitch { get; }

        public int Velocity { get; }

        [NotNull]
        public string Start { get; }

        [NotNull]
        public string End { get; }
    }
}
