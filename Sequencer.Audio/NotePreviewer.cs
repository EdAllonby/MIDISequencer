using System;
using System.Linq;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// Plays notes and drum hits on the instrument of the track being edited.
    /// </summary>
    public sealed class NotePreviewer : INotePreviewer
    {
        [NotNull] private readonly SequencerPlayer player;
        [NotNull] private readonly EditingSettings editingSettings;
        [NotNull] private readonly ArrangementState state;
        [NotNull] private readonly Song song;

        public NotePreviewer([NotNull] SequencerPlayer player, [NotNull] EditingSettings editingSettings, [NotNull] ArrangementState state, [NotNull] Song song)
        {
            this.player = player;
            this.editingSettings = editingSettings;
            this.state = state;
            this.song = song;
        }

        public void Preview(Tone tone)
        {
            Track track = TrackOfKind(TrackKind.Synth);

            if (editingSettings.PreviewOnEdit && track != null)
            {
                player.Preview(tone, track);
            }
        }

        public IDisposable Hold(Tone tone)
        {
            Track track = TrackOfKind(TrackKind.Synth);
            return track != null ? player.Hold(tone, track) : new Nothing();
        }

        public void PreviewDrum(DrumSound sound, byte velocity)
        {
            if (editingSettings.PreviewOnEdit)
            {
                PlayDrum(sound, velocity);
            }
        }

        public void PlayDrum(DrumSound sound, byte velocity)
        {
            Track track = TrackOfKind(TrackKind.Drums);

            if (track != null)
            {
                player.PreviewDrum(sound, velocity, track);
            }
        }

        /// <summary>
        /// The selected track if it is this kind, otherwise the first track that is.
        /// </summary>
        [CanBeNull]
        private Track TrackOfKind(TrackKind kind)
        {
            Track selected = state.SelectedTrack;
            return selected?.Kind == kind ? selected : song.Tracks.FirstOrDefault(track => track.Kind == kind);
        }

        private sealed class Nothing : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
