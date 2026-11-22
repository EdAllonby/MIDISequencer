using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// A read-only summary of the selected note, for the inspector panel.
    /// </summary>
    public sealed class SelectedNoteViewModel
    {
        private const int TicksPerQuarterNote = 96;

        public SelectedNoteViewModel([NotNull] Tone tone)
        {
            TimeSignature timeSignature = TimeSignature.FourFour;
            Pitch = tone.Pitch.ToString();
            Velocity = tone.Velocity.Value;
            VelocityPercent = tone.Velocity.Volume * 100;
            Start = MusicalFormat.Position(tone.StartPosition, timeSignature, TicksPerQuarterNote);
            End = MusicalFormat.Position(tone.EndPosition, timeSignature, TicksPerQuarterNote);

            int lengthInTicks = tone.EndPosition.TotalTicks(timeSignature, TicksPerQuarterNote) - tone.StartPosition.TotalTicks(timeSignature, TicksPerQuarterNote);
            Length = MusicalFormat.Length(lengthInTicks / (TicksPerQuarterNote / 4));
        }

        [NotNull]
        public string Pitch { get; }

        /// <summary>
        /// MIDI velocity, 0 to 127.
        /// </summary>
        public int Velocity { get; }

        public double VelocityPercent { get; }

        /// <summary>
        /// "bar.beat.sixteenth".
        /// </summary>
        [NotNull]
        public string Start { get; }

        [NotNull]
        public string End { get; }

        /// <summary>
        /// As a musical fraction, such as "1/8".
        /// </summary>
        [NotNull]
        public string Length { get; }
    }
}
