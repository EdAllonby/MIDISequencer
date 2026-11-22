using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Arrangement
{
    public enum TrackKind
    {
        Synth,
        Drums
    }

    /// <summary>
    /// One row of the arrangement: an instrument and the clips it plays in each section.
    /// </summary>
    public sealed class Track
    {
        [NotNull] [ItemCanBeNull] private readonly Clip[] sections;
        [NotNull] [ItemNotNull] private readonly List<Clip> clips = new List<Clip>();
        private volatile bool isMuted;

        public Track(TrackKind kind, [NotNull] string name, int colourIndex, int sectionCount)
        {
            Kind = kind;
            Name = name;
            ColourIndex = colourIndex;
            sections = new Clip[sectionCount];
            Synth = kind == TrackKind.Synth ? new SynthSettings() : null;
            Drums = kind == TrackKind.Drums ? new DrumKitSettings() : null;
        }

        public TrackKind Kind { get; }

        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Which of the arrangement's track colours to use.
        /// </summary>
        public int ColourIndex { get; }

        /// <summary>
        /// The synth's sound, for a synth track; otherwise null.
        /// </summary>
        [CanBeNull]
        public SynthSettings Synth { get; }

        /// <summary>
        /// The drum kit's settings, for a drum track; otherwise null.
        /// </summary>
        [CanBeNull]
        public DrumKitSettings Drums { get; }

        public bool IsMuted
        {
            get => isMuted;
            set => isMuted = value;
        }

        /// <summary>
        /// Every clip made on this track, placed or not.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Clip> Clips => clips;

        public int SectionCount => sections.Length;

        /// <summary>
        /// The clip in a section, or null. Safe to call from the clock thread.
        /// </summary>
        [CanBeNull]
        public Clip ClipAt(int section) => section >= 0 && section < sections.Length ? sections[section] : null;

        internal void Place(int section, [CanBeNull] Clip clip)
        {
            sections[section] = clip;
        }

        internal void AddClip([NotNull] Clip clip)
        {
            clips.Add(clip);
        }

        /// <summary>
        /// The next free clip name: A, B, C ... Z, then AA, AB.
        /// </summary>
        [NotNull]
        internal string NextClipName()
        {
            for (var index = 0;; index++)
            {
                string name = index < 26 ? ((char) ('A' + index)).ToString() : $"{(char) ('A' + index / 26 - 1)}{(char) ('A' + index % 26)}";

                if (clips.All(clip => clip.Name != name))
                {
                    return name;
                }
            }
        }

        public override string ToString() => Name;
    }
}
