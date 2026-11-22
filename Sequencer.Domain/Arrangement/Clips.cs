using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;

namespace Sequencer.Domain.Arrangement
{
    /// <summary>
    /// A four-bar pattern on a track. One clip can be placed in several sections; editing it changes all of them.
    /// </summary>
    public abstract class Clip
    {
        protected Clip([NotNull] Track track, [NotNull] string name)
        {
            Track = track;
            Name = name;
        }

        [NotNull]
        public Track Track { get; }

        /// <summary>
        /// A short name such as "A", unique on its track.
        /// </summary>
        [NotNull]
        public string Name { get; }

        public abstract bool IsEmpty { get; }

        public override string ToString() => $"{Track.Name} {Name}";
    }

    /// <summary>
    /// A clip of synth notes. While it is open in the piano roll, playback reads the editor's notes directly,
    /// so edits are heard straight away; when it closes, it keeps a copy.
    /// </summary>
    public sealed class NoteClip : Clip, ISequencedNotes
    {
        [NotNull] private volatile StoredNote[] stored = Array.Empty<StoredNote>();
        [CanBeNull] private volatile ISequencedNotes live;

        public NoteClip([NotNull] Track track, [NotNull] string name) : base(track, name)
        {
        }

        public bool IsOpen => live != null;

        public override bool IsEmpty => !Tones.Any();

        /// <summary>
        /// The clip's notes (from the editor while it is open).
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Tone> Tones
        {
            get
            {
                ISequencedNotes source = live;
                return source != null ? source.AllNotes.Select(note => note.Tone).ToList() : stored.Select(note => note.Tone).ToList();
            }
        }

        public void SetTones([NotNull] [ItemNotNull] IEnumerable<Tone> tones)
        {
            stored = tones.Select(tone => new StoredNote(tone)).ToArray();
        }

        /// <summary>
        /// Plays from the editor's notes while the clip is open in it.
        /// </summary>
        public void Open([NotNull] ISequencedNotes editorNotes)
        {
            live = editorNotes;
        }

        /// <summary>
        /// Keeps a copy of the editor's notes and stops reading from it.
        /// </summary>
        public void Close()
        {
            ISequencedNotes source = live;

            if (source != null)
            {
                SetTones(source.AllNotes.Select(note => note.Tone));
                live = null;
            }
        }

        [NotNull]
        public NoteClip CopyAs([NotNull] string name)
        {
            var copy = new NoteClip(Track, name);
            copy.SetTones(Tones);
            return copy;
        }

        public IEnumerable<ISequencedNote> AllNotes => (IEnumerable<ISequencedNote>) live?.AllNotes ?? stored;

        public IEnumerable<ISequencedNote> FindNotesFromStartingPosition(IPosition position)
        {
            ISequencedNotes source = live;
            return source != null ? source.FindNotesFromStartingPosition(position) : stored.Where(note => note.Tone.StartPosition.Equals(position)).ToList();
        }

        public IEnumerable<ISequencedNote> FindNotesFromEndingPosition(IPosition position)
        {
            ISequencedNotes source = live;
            return source != null ? source.FindNotesFromEndingPosition(position) : stored.Where(note => note.Tone.EndPosition.Equals(position)).ToList();
        }

        /// <summary>
        /// A note kept while the clip isn't open. Its identity lets playback match a note's start and end.
        /// </summary>
        private sealed class StoredNote : ISequencedNote
        {
            public StoredNote([NotNull] Tone tone)
            {
                Tone = tone;
            }

            public Tone Tone { get; }
        }
    }

    /// <summary>
    /// A clip of drum hits.
    /// </summary>
    public sealed class DrumClip : Clip
    {
        public DrumClip([NotNull] Track track, [NotNull] string name, [CanBeNull] DrumPattern pattern = null) : base(track, name)
        {
            Pattern = pattern ?? new DrumPattern();
        }

        [NotNull]
        public DrumPattern Pattern { get; }

        public override bool IsEmpty => Pattern.IsEmpty;

        [NotNull]
        public DrumClip CopyAs([NotNull] string name) => new DrumClip(Track, name, Pattern.Clone());
    }
}
