using System.Drawing;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// Colours for everything drawn on the editor canvas, the ruler and the keyboard.
    /// </summary>
    public interface IColourSettings
    {
        // Note lanes: natural notes are slightly lighter than sharps and flats, as on a keyboard.
        Color LaneColour { get; }
        Color AccidentalLaneColour { get; }

        // Grid lines, faintest to strongest.
        Color SixteenthLineColour { get; }
        Color BeatLineColour { get; }
        Color BarLineColour { get; }

        /// <summary>
        /// The line under each C, for finding your octave.
        /// </summary>
        Color OctaveLineColour { get; }

        Color NoteColour { get; }
        Color SelectedNoteColour { get; }

        /// <summary>
        /// Separates touching notes.
        /// </summary>
        Color NoteOutlineColour { get; }

        Color SelectedNoteOutlineColour { get; }

        /// <summary>
        /// The velocity strip along the bottom of each note.
        /// </summary>
        Color VelocityColour { get; }

        Color PlayheadColour { get; }

        /// <summary>
        /// Keys lit by playback, and the pressed key.
        /// </summary>
        Color ActiveKeyColour { get; }

        Color WhiteKeyColour { get; }
        Color BlackKeyColour { get; }
        Color KeyLabelColour { get; }

        Color RulerTextColour { get; }
        Color RulerTickColour { get; }

        Color SelectionColour { get; }
    }
}
