﻿using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi
{
    public class TickCalculator : ITickCalculator
    {
        private readonly int quarterNoteResolution;
        [NotNull] private readonly TimeSignature timeSignature;

        public TickCalculator([NotNull] IMusicalSettings musicalSettings)
        {
            timeSignature = musicalSettings.TimeSignature;
            quarterNoteResolution = musicalSettings.TicksPerQuarterNote;
        }

        public IPosition CalculatePositionFromTick(int tick)
        {
            return Position.PositionFromTick(tick, timeSignature, quarterNoteResolution);
        }
    }
}