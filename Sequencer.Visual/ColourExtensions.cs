using Avalonia.Media;

namespace Sequencer.Visual
{
    public static class ColourExtensions
    {
        public static Color ToAvalonia(this System.Drawing.Color colour)
        {
            return Color.FromArgb(colour.A, colour.R, colour.G, colour.B);
        }

        public static IBrush ToBrush(this System.Drawing.Color colour)
        {
            return new SolidColorBrush(colour.ToAvalonia());
        }
    }
}
