using System;
using System.Globalization;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;

namespace Sequencer.View.Control
{
    /// <summary>
    /// A step-sequencer grid for the drum kit: one row per sound, one column per sixteenth. Click a step to
    /// toggle it (⌥/Alt-click for an accent), drag to paint, and click a sound's name to hear it.
    /// </summary>
    public sealed class DrumEditorControl : Avalonia.Controls.Control
    {
        /// <summary>
        /// Matches the piano keyboard's width, so the ruler above lines up with both editors.
        /// </summary>
        public const double LabelWidth = 72;

        public const double RowHeight = 40;

        public static readonly StyledProperty<int> CurrentStepProperty =
            AvaloniaProperty.Register<DrumEditorControl, int>(nameof(CurrentStep), -1);

        [NotNull] private static readonly IBrush LabelBackground = new SolidColorBrush(Color.Parse("#1C1E22"));
        [NotNull] private static readonly IBrush LabelText = new SolidColorBrush(Color.Parse("#9A9EA5"));
        [NotNull] private static readonly IBrush EvenBeatCell = new SolidColorBrush(Color.Parse("#2A2D33"));
        [NotNull] private static readonly IBrush OddBeatCell = new SolidColorBrush(Color.Parse("#222428"));
        [NotNull] private static readonly IBrush HitBrush = new SolidColorBrush(Color.Parse("#E8A33D"), 0.72);
        [NotNull] private static readonly IBrush AccentBrush = new SolidColorBrush(Color.Parse("#FFC062"));
        [NotNull] private static readonly IBrush PlayheadBrush = new SolidColorBrush(Colors.White, 0.12);
        [NotNull] private static readonly Pen BarLine = new Pen(new SolidColorBrush(Color.Parse("#50545D")), 1);
        [NotNull] private static readonly Pen RowLine = new Pen(new SolidColorBrush(Color.Parse("#16171A")), 1);

        [CanBeNull] private DrumPattern pattern;
        private byte? paintVelocity;
        private DrumSound? paintedSound;
        private int paintedStep = -1;

        public DrumEditorControl()
        {
            Height = RowHeight * DrumPattern.AllSounds.Length;
            ClipToBounds = true;
            Cursor = new Cursor(StandardCursorType.Hand);
        }

        /// <summary>
        /// Called when a step is turned on, to preview it.
        /// </summary>
        [CanBeNull]
        public Action<DrumSound, byte> PreviewSound { get; set; }

        /// <summary>
        /// Called when a sound's name is clicked, to hear it.
        /// </summary>
        [CanBeNull]
        public Action<DrumSound, byte> AuditionSound { get; set; }

        /// <summary>
        /// The step being played, highlighted in the grid; -1 for none.
        /// </summary>
        public int CurrentStep
        {
            get => GetValue(CurrentStepProperty);
            set => SetValue(CurrentStepProperty, value);
        }

        [CanBeNull]
        public DrumPattern Pattern
        {
            get => pattern;
            set
            {
                if (pattern != null)
                {
                    pattern.Changed -= OnPatternChanged;
                }

                pattern = value;

                if (pattern != null)
                {
                    pattern.Changed += OnPatternChanged;
                }

                InvalidateVisual();
            }
        }

        static DrumEditorControl()
        {
            AffectsRender<DrumEditorControl>(CurrentStepProperty);
        }

        private double CellWidth => Math.Max(1, (Bounds.Width - LabelWidth) / DrumPattern.StepCount);

        public override void Render([NotNull] DrawingContext context)
        {
            DrumSound[] sounds = DrumPattern.AllSounds;
            double cellWidth = CellWidth;
            context.FillRectangle(LabelBackground, new Rect(0, 0, LabelWidth, Bounds.Height));

            for (var row = 0; row < sounds.Length; row++)
            {
                double top = row * RowHeight;
                var label = new FormattedText(NameOf(sounds[row]), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, LabelText);
                context.DrawText(label, new Point(10, top + (RowHeight - label.Height) / 2));

                for (var step = 0; step < DrumPattern.StepCount; step++)
                {
                    double left = LabelWidth + step * cellWidth;
                    var cell = new Rect(left + 1.5, top + 5, Math.Max(1, cellWidth - 3), RowHeight - 10);
                    byte velocity = pattern?.VelocityAt(sounds[row], step) ?? 0;

                    IBrush fill = velocity >= DrumPattern.AccentVelocity ? AccentBrush
                        : velocity > 0 ? HitBrush
                        : step / 4 % 2 == 0 ? EvenBeatCell : OddBeatCell;

                    context.FillRectangle(fill, cell, 2);

                    if (step == CurrentStep)
                    {
                        context.FillRectangle(PlayheadBrush, cell, 2);
                    }
                }

                double rowBottom = Math.Floor(top + RowHeight) + 0.5;
                context.DrawLine(RowLine, new Point(LabelWidth, rowBottom), new Point(Bounds.Width, rowBottom));
            }

            for (var bar = 1; bar < DrumPattern.StepCount / 16; bar++)
            {
                double x = Math.Floor(LabelWidth + bar * 16 * cellWidth) + 0.5;
                context.DrawLine(BarLine, new Point(x, 0), new Point(x, Bounds.Height));
            }
        }

        [NotNull]
        public static string NameOf(DrumSound sound)
        {
            switch (sound)
            {
                case DrumSound.OpenHat: return "Open hat";
                case DrumSound.ClosedHat: return "Closed hat";
                case DrumSound.Clap: return "Clap";
                case DrumSound.Snare: return "Snare";
                case DrumSound.Tom: return "Tom";
                case DrumSound.Kick: return "Kick";
                default: return sound.ToString();
            }
        }

        protected override void OnPointerPressed([NotNull] PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (pattern == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                return;
            }

            Point point = e.GetPosition(this);
            DrumSound? sound = SoundAt(point.Y);

            if (sound == null)
            {
                return;
            }

            if (point.X < LabelWidth)
            {
                AuditionSound?.Invoke(sound.Value, DrumPattern.NormalVelocity);
                e.Handled = true;
                return;
            }

            int step = StepAt(point.X);
            bool accent = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

            // The first cell decides whether this drag adds hits or erases them.
            paintVelocity = pattern.IsHit(sound.Value, step) && !accent ? (byte) 0
                : accent ? DrumPattern.AccentVelocity
                : DrumPattern.NormalVelocity;

            Paint(sound.Value, step, true);
            e.Pointer.Capture(this);
            e.Handled = true;
        }

        protected override void OnPointerMoved([NotNull] PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (paintVelocity == null)
            {
                return;
            }

            Point point = e.GetPosition(this);
            DrumSound? sound = SoundAt(point.Y);

            if (sound != null && point.X >= LabelWidth)
            {
                Paint(sound.Value, StepAt(point.X), false);
            }
        }

        protected override void OnPointerReleased([NotNull] PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            paintVelocity = null;
            paintedSound = null;
            paintedStep = -1;
            e.Pointer.Capture(null);
        }

        private void Paint(DrumSound sound, int step, bool preview)
        {
            if (pattern == null || paintVelocity == null || (paintedSound == sound && paintedStep == step))
            {
                return;
            }

            paintedSound = sound;
            paintedStep = step;
            pattern.SetVelocity(sound, step, paintVelocity.Value);

            if (preview && paintVelocity.Value > 0)
            {
                PreviewSound?.Invoke(sound, paintVelocity.Value);
            }
        }

        private DrumSound? SoundAt(double y)
        {
            var row = (int) Math.Floor(y / RowHeight);
            return row >= 0 && row < DrumPattern.AllSounds.Length ? DrumPattern.AllSounds[row] : (DrumSound?) null;
        }

        private int StepAt(double x)
        {
            return Math.Clamp((int) Math.Floor((x - LabelWidth) / CellWidth), 0, DrumPattern.StepCount - 1);
        }

        private void OnPatternChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }
    }
}
