using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Effects;
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

        // Copy-on-write, so the audio thread can read the chain without a lock.
        [NotNull] [ItemNotNull] private volatile Effect[] effects = Array.Empty<Effect>();

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

        /// <summary>
        /// The track's effects, in the order the sound passes through them. Safe to read from the audio thread.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Effect> Effects => effects;

        /// <summary>
        /// Raised when an effect is added, removed, moved, switched on or off, or a parameter changes.
        /// </summary>
        public event EventHandler EffectsChanged;

        public void AddEffect([NotNull] Effect effect)
        {
            InsertEffect(effects.Length, effect);
        }

        public void InsertEffect(int index, [NotNull] Effect effect)
        {
            if (effects.Contains(effect))
            {
                throw new InvalidOperationException($"{effect.Name} is already on {Name}.");
            }

            List<Effect> chain = effects.ToList();
            chain.Insert(Math.Clamp(index, 0, chain.Count), effect);
            effect.Changed += OnEffectChanged;
            SetEffects(chain);
        }

        public void RemoveEffect([NotNull] Effect effect)
        {
            if (!effects.Contains(effect))
            {
                return;
            }

            effect.Changed -= OnEffectChanged;
            SetEffects(effects.Where(e => e != effect).ToList());
        }

        /// <summary>
        /// Moves an effect earlier (negative) or later (positive) in the chain.
        /// </summary>
        public void MoveEffect([NotNull] Effect effect, int offset)
        {
            List<Effect> chain = effects.ToList();
            int from = chain.IndexOf(effect);
            int to = Math.Clamp(from + offset, 0, chain.Count - 1);

            if (from < 0 || from == to)
            {
                return;
            }

            chain.RemoveAt(from);
            chain.Insert(to, effect);
            SetEffects(chain);
        }

        private void SetEffects([NotNull] [ItemNotNull] List<Effect> chain)
        {
            effects = chain.ToArray();
            EffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnEffectChanged(object sender, EventArgs e)
        {
            EffectsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString() => Name;
    }
}
