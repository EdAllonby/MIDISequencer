using Avalonia.Media;
using JetBrains.Annotations;

namespace Sequencer.View
{
    /// <summary>
    /// The app's icons, drawn on a 16 x 16 grid. Tool icons are outlines (stroke them); transport icons are
    /// solid shapes (fill them).
    /// </summary>
    public static class Icons
    {
        [NotNull] public static readonly Geometry Select = Geometry.Parse("M3.5,2 L3.5,13 L6.4,10.3 L8.4,14.5 L10.2,13.7 L8.3,9.6 L12,9.4 Z");

        [NotNull] public static readonly Geometry Draw = Geometry.Parse("M10.8,2.6 L13.4,5.2 L5.6,13 L2.5,13.5 L3,10.4 Z M9.2,4.2 L11.8,6.8");

        [NotNull] public static readonly Geometry Erase = Geometry.Parse("M9.3,2.8 L13.7,7.2 L7.9,13 L4.6,13 L2.3,10.7 Z M6.1,6 L10.5,10.4 M7.9,13 L14,13");

        [NotNull] public static readonly Geometry Play = Geometry.Parse("M4.5,2.8 L13,8 L4.5,13.2 Z");

        [NotNull] public static readonly Geometry Pause = Geometry.Parse("M4,3 L7,3 L7,13 L4,13 Z M9,3 L12,3 L12,13 L9,13 Z");

        [NotNull] public static readonly Geometry Stop = Geometry.Parse("M3.5,3.5 L12.5,3.5 L12.5,12.5 L3.5,12.5 Z");

        // Waveforms, drawn as one and a half to two cycles in a 20 x 14 box.
        [NotNull] public static readonly Geometry Sawtooth = Geometry.Parse("M1,12 L9,2 L9,12 L17,2 L17,12");

        [NotNull] public static readonly Geometry Square = Geometry.Parse("M1,12 L1,2 L6,2 L6,12 L11,12 L11,2 L16,2 L16,12 L19,12");

        [NotNull] public static readonly Geometry Triangle = Geometry.Parse("M1,12 L5.5,2 L10,12 L14.5,2 L19,12");

        [NotNull] public static readonly Geometry Sine = Geometry.Parse("M1,7 C3,0.5 5,0.5 7,7 C9,13.5 11,13.5 13,7 C15,0.5 17,0.5 19,7");

        /// <summary>
        /// The icon for a note action's icon name.
        /// </summary>
        [CanBeNull]
        public static Geometry ForTool([CanBeNull] string iconName)
        {
            switch (iconName)
            {
                case "Select": return Select;
                case "Draw": return Draw;
                case "Erase": return Erase;
                default: return null;
            }
        }
    }
}
