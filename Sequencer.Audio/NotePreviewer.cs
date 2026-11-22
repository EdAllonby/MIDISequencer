using System;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// Previews notes through the player, if the user has previews switched on.
    /// </summary>
    public sealed class NotePreviewer : INotePreviewer
    {
        [NotNull] private readonly SequencerPlayer player;
        [NotNull] private readonly SynthSettings settings;

        public NotePreviewer([NotNull] SequencerPlayer player, [NotNull] SynthSettings settings)
        {
            this.player = player;
            this.settings = settings;
        }

        public void Preview(Tone tone)
        {
            if (settings.PreviewOnEdit)
            {
                player.Preview(tone);
            }
        }

        public IDisposable Hold(Tone tone)
        {
            return player.Hold(tone);
        }
    }
}
