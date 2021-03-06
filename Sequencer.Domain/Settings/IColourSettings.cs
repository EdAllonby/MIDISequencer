﻿using System.Windows.Media;

namespace Sequencer.Domain.Settings
{
    public interface IColourSettings
    {
        Color AccidentalKeyColour { get; }
        Color KeyColour { get; }
        Color LineColour { get; }
        Color SelectedNoteColour { get; }
        Color UnselectedNoteColour { get; }
        Color IndicatorColour { get; }
    }
}