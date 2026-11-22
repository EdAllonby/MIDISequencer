using System;
using System.Linq;
using JetBrains.Annotations;

namespace Sequencer.Domain.Drums
{
    /// <summary>
    /// A drum pattern: for each sound, which sixteenth steps it plays on and how hard. A step's velocity is 0
    /// when it doesn't play. Edited on the UI thread and read by playback on the clock thread; each step is a
    /// single byte, so a reader sees either the old or the new value.
    /// </summary>
    public sealed class DrumPattern
    {
        public const int StepCount = 64;
        public const byte NormalVelocity = 96;
        public const byte AccentVelocity = 127;

        [NotNull] private static readonly DrumSound[] Sounds = (DrumSound[]) Enum.GetValues(typeof(DrumSound));
        [NotNull] private readonly byte[][] steps;

        public DrumPattern()
        {
            steps = Sounds.Select(sound => new byte[StepCount]).ToArray();
        }

        [NotNull]
        public static DrumSound[] AllSounds => Sounds;

        /// <summary>
        /// Raised on the UI thread after any change.
        /// </summary>
        public event EventHandler Changed;

        public byte VelocityAt(DrumSound sound, int step) => steps[(int) sound][step];

        public bool IsHit(DrumSound sound, int step) => VelocityAt(sound, step) > 0;

        public bool IsEmpty => steps.All(lane => lane.All(velocity => velocity == 0));

        public int HitCount => steps.Sum(lane => lane.Count(velocity => velocity > 0));

        public void SetVelocity(DrumSound sound, int step, byte velocity)
        {
            if (steps[(int) sound][step] == velocity)
            {
                return;
            }

            steps[(int) sound][step] = velocity;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            foreach (byte[] lane in steps)
            {
                Array.Clear(lane, 0, lane.Length);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Replaces this pattern's steps with another's.
        /// </summary>
        public void CopyFrom([NotNull] DrumPattern other)
        {
            for (var lane = 0; lane < steps.Length; lane++)
            {
                Array.Copy(other.steps[lane], steps[lane], StepCount);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        [NotNull]
        public DrumPattern Clone()
        {
            var copy = new DrumPattern();
            copy.CopyFrom(this);
            return copy;
        }
    }
}
