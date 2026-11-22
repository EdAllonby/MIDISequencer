using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sequencer.Domain.Arrangement
{
    /// <summary>
    /// The arrangement: tracks of clips, laid out in four-bar sections. Edited on the UI thread; playback reads
    /// the tracks and their sections from the clock thread.
    /// </summary>
    public sealed class Song
    {
        public const int DefaultSectionCount = 8;
        public const int MaximumTracks = 8;

        [NotNull] [ItemNotNull] private volatile Track[] tracks = Array.Empty<Track>();

        public Song(int sectionCount = DefaultSectionCount)
        {
            SectionCount = sectionCount;
        }

        /// <summary>
        /// Raised on the UI thread when tracks, clips or placements change.
        /// </summary>
        public event EventHandler Changed;

        public int SectionCount { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Track> Tracks => tracks;

        /// <summary>
        /// Up to and including the last section with a clip in it; at least one.
        /// </summary>
        public int LengthInSections
        {
            get
            {
                int last = Enumerable.Range(0, SectionCount).LastOrDefault(section => tracks.Any(track => track.ClipAt(section) != null));
                return last + 1;
            }
        }

        /// <summary>
        /// A song with one synth track and one drum track, each with an empty clip in the first section.
        /// </summary>
        [NotNull]
        public static Song CreateDefault()
        {
            var song = new Song();
            Track synth = song.AddTrack(TrackKind.Synth);
            Track drums = song.AddTrack(TrackKind.Drums);
            song.Place(synth, 0, song.CreateClip(synth));
            song.Place(drums, 0, song.CreateClip(drums));
            return song;
        }

        [NotNull]
        public Track AddTrack(TrackKind kind)
        {
            if (tracks.Length >= MaximumTracks)
            {
                throw new InvalidOperationException($"A song can have up to {MaximumTracks} tracks.");
            }

            int sameKind = tracks.Count(track => track.Kind == kind);
            string name = kind == TrackKind.Synth ? "Synth" : "Drums";
            var track = new Track(kind, sameKind == 0 ? name : $"{name} {sameKind + 1}", tracks.Length, SectionCount);

            // Copy-on-write, so playback always sees a complete array.
            tracks = tracks.Concat(new[] { track }).ToArray();
            Changed?.Invoke(this, EventArgs.Empty);
            return track;
        }

        /// <summary>
        /// A new, empty clip on the track, named with its next free letter. Not placed anywhere yet.
        /// </summary>
        [NotNull]
        public Clip CreateClip([NotNull] Track track)
        {
            string name = track.NextClipName();
            Clip clip = track.Kind == TrackKind.Synth ? (Clip) new NoteClip(track, name) : new DrumClip(track, name);
            track.AddClip(clip);
            Changed?.Invoke(this, EventArgs.Empty);
            return clip;
        }

        /// <summary>
        /// A new clip with a copy of another's contents.
        /// </summary>
        [NotNull]
        public Clip CopyClip([NotNull] Clip clip)
        {
            string name = clip.Track.NextClipName();
            Clip copy = clip is NoteClip notes ? (Clip) notes.CopyAs(name) : ((DrumClip) clip).CopyAs(name);
            clip.Track.AddClip(copy);
            Changed?.Invoke(this, EventArgs.Empty);
            return copy;
        }

        /// <summary>
        /// Puts a clip (or nothing) in a section of its track.
        /// </summary>
        public void Place([NotNull] Track track, int section, [CanBeNull] Clip clip)
        {
            if (clip != null && clip.Track != track)
            {
                throw new ArgumentException("A clip can only be placed on its own track.", nameof(clip));
            }

            track.Place(section, clip);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Replaces every track with another song's: how a new or loaded song takes over the one the app is
        /// playing, so everything holding this song sees the change.
        /// </summary>
        public void ReplaceWith([NotNull] Song other)
        {
            ReplaceTracks(other.Tracks);
        }

        internal void ReplaceTracks([NotNull] [ItemNotNull] IEnumerable<Track> replacement)
        {
            tracks = replacement.Take(MaximumTracks).ToArray();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Lets views redraw after a clip's contents change.
        /// </summary>
        public void NotifyClipEdited()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
