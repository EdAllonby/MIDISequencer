using System;
using Avalonia;
using Avalonia.Media;
using JetBrains.Annotations;

namespace Sequencer.View.Control
{
    /// <summary>
    /// Draws the attack, decay, sustain and release envelope, so the sliders' effect is visible as a shape.
    /// </summary>
    public sealed class EnvelopeGraph : Avalonia.Controls.Control
    {
        public static readonly StyledProperty<int> AttackMillisecondsProperty =
            AvaloniaProperty.Register<EnvelopeGraph, int>(nameof(AttackMilliseconds));

        public static readonly StyledProperty<int> DecayMillisecondsProperty =
            AvaloniaProperty.Register<EnvelopeGraph, int>(nameof(DecayMilliseconds));

        public static readonly StyledProperty<int> SustainPercentProperty =
            AvaloniaProperty.Register<EnvelopeGraph, int>(nameof(SustainPercent));

        public static readonly StyledProperty<int> ReleaseMillisecondsProperty =
            AvaloniaProperty.Register<EnvelopeGraph, int>(nameof(ReleaseMilliseconds));

        public static readonly StyledProperty<IBrush> StrokeProperty =
            AvaloniaProperty.Register<EnvelopeGraph, IBrush>(nameof(Stroke), Brushes.White);

        // The sustain plateau has no length of its own (it lasts as long as the note), so it gets a fixed share.
        private const double SustainShare = 0.24;
        private const double Padding = 3;

        static EnvelopeGraph()
        {
            AffectsRender<EnvelopeGraph>(AttackMillisecondsProperty, DecayMillisecondsProperty, SustainPercentProperty,
                ReleaseMillisecondsProperty, StrokeProperty);
        }

        public int AttackMilliseconds
        {
            get => GetValue(AttackMillisecondsProperty);
            set => SetValue(AttackMillisecondsProperty, value);
        }

        public int DecayMilliseconds
        {
            get => GetValue(DecayMillisecondsProperty);
            set => SetValue(DecayMillisecondsProperty, value);
        }

        public int SustainPercent
        {
            get => GetValue(SustainPercentProperty);
            set => SetValue(SustainPercentProperty, value);
        }

        public int ReleaseMilliseconds
        {
            get => GetValue(ReleaseMillisecondsProperty);
            set => SetValue(ReleaseMillisecondsProperty, value);
        }

        public IBrush Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        /// <summary>
        /// Draws from one point to another with the given shape (0 at the start, 1 at the end).
        /// </summary>
        private static void Curve([NotNull] StreamGeometryContext geometry, Point from, Point to, [NotNull] Func<double, double> shape)
        {
            const int steps = 16;

            for (var step = 1; step <= steps; step++)
            {
                double t = (double) step / steps;
                geometry.LineTo(new Point(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * shape(t)));
            }
        }

        public override void Render([NotNull] DrawingContext context)
        {
            double width = Bounds.Width - Padding * 2;
            double height = Bounds.Height - Padding * 2;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            // Square roots keep a 5 ms attack visible next to a 2 s release.
            double attack = Math.Sqrt(Math.Max(1, AttackMilliseconds));
            double decay = Math.Sqrt(Math.Max(1, DecayMilliseconds));
            double release = Math.Sqrt(Math.Max(1, ReleaseMilliseconds));
            double timeWidth = width * (1 - SustainShare) / (attack + decay + release);

            double bottom = Padding + height;
            double sustainY = Padding + height * (1 - Math.Clamp(SustainPercent, 0, 100) / 100.0);
            var start = new Point(Padding, bottom);
            var peak = new Point(start.X + attack * timeWidth, Padding);
            var sustainStart = new Point(peak.X + decay * timeWidth, sustainY);
            var sustainEnd = new Point(sustainStart.X + width * SustainShare, sustainY);
            var end = new Point(Padding + width, bottom);

            var outline = new StreamGeometry();

            using (StreamGeometryContext geometry = outline.Open())
            {
                // Curves shaped like the real envelope: the attack rises quickly then eases into the peak;
                // decay and release fall fast at first, then tail off.
                geometry.BeginFigure(start, true);
                Curve(geometry, start, peak, t => (1 - Math.Exp(-2.5 * t)) / (1 - Math.Exp(-2.5)));
                Curve(geometry, peak, sustainStart, t => (1 - Math.Exp(-5 * t)) / (1 - Math.Exp(-5)));
                geometry.LineTo(sustainEnd);
                Curve(geometry, sustainEnd, end, t => (1 - Math.Exp(-5 * t)) / (1 - Math.Exp(-5)));
                geometry.EndFigure(false);
            }

            Color colour = Stroke is ISolidColorBrush solid ? solid.Color : Colors.White;
            context.DrawGeometry(new SolidColorBrush(colour, 0.14), null, outline);
            context.DrawGeometry(null, new Pen(Stroke, 1.5, lineJoin: PenLineJoin.Round, lineCap: PenLineCap.Round), outline);
        }
    }
}
