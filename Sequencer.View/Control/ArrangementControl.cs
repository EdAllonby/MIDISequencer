using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The arrangement: one row per track, one column per four-bar section, and a clip (or nothing) in each
    /// cell. Click a cell to edit it, double-click an empty one to make a clip, right-click for more.
    /// </summary>
    public sealed class ArrangementControl : Avalonia.Controls.Control
    {
        public const double HeaderWidth = 168;
        public const double RulerHeight = 26;
        public const double RowHeight = 42;
        public const double AddRowHeight = 36;

        public static readonly StyledProperty<int> PlayheadTickProperty =
            AvaloniaProperty.Register<ArrangementControl, int>(nameof(PlayheadTick), -1);

        /// <summary>
        /// Track colours, in order of adding. Saturated enough to tell apart, muted enough to sit on dark grey.
        /// </summary>
        [NotNull] public static readonly Color[] TrackColours =
        {
            Color.Parse("#E8A33D"), Color.Parse("#4FB3A5"), Color.Parse("#9B87F5"), Color.Parse("#E06C8E"),
            Color.Parse("#5B9BE6"), Color.Parse("#8DBF5A"), Color.Parse("#D98452"), Color.Parse("#C77DC9")
        };

        [NotNull] private static readonly IBrush Background = new SolidColorBrush(Color.Parse("#16171A"));
        [NotNull] private static readonly IBrush HeaderBrush = new SolidColorBrush(Color.Parse("#1C1E22"));
        [NotNull] private static readonly IBrush SelectedHeaderBrush = new SolidColorBrush(Color.Parse("#24272C"));
        [NotNull] private static readonly IBrush EmptyCellBrush = new SolidColorBrush(Color.Parse("#1F2125"));
        [NotNull] private static readonly IBrush PrimaryText = new SolidColorBrush(Color.Parse("#E6E7E9"));
        [NotNull] private static readonly IBrush SecondaryText = new SolidColorBrush(Color.Parse("#9A9EA5"));
        [NotNull] private static readonly IBrush TertiaryText = new SolidColorBrush(Color.Parse("#6A6E75"));
        [NotNull] private static readonly IBrush DarkText = new SolidColorBrush(Color.Parse("#16171A"));
        [NotNull] private static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#E8A33D"));
        [NotNull] private static readonly IBrush LoopColumn = new SolidColorBrush(Color.Parse("#E8A33D"), 0.07);
        [NotNull] private static readonly IBrush Raised = new SolidColorBrush(Color.Parse("#2A2D33"));
        [NotNull] private static readonly Pen SelectionPen = new Pen(Brushes.White, 2);
        [NotNull] private static readonly Pen PlayheadPen = new Pen(new SolidColorBrush(Color.Parse("#F2F3F5")), 1);
        [NotNull] private static readonly Pen HairlinePen = new Pen(new SolidColorBrush(Color.Parse("#2C2F35")), 1);

        [CanBeNull] private Song song;
        [CanBeNull] private ArrangementState state;

        static ArrangementControl()
        {
            AffectsRender<ArrangementControl>(PlayheadTickProperty);
        }

        public ArrangementControl()
        {
            ClipToBounds = true;
        }

        public event EventHandler<CellEventArgs> CellClicked;

        public event EventHandler<CellEventArgs> CellDoubleClicked;

        public event EventHandler<CellEventArgs> CellContextRequested;

        public event EventHandler<Track> MuteToggled;

        public event EventHandler<TrackKind> AddTrackRequested;

        /// <summary>
        /// The playback position in the song, in ticks; -1 when stopped.
        /// </summary>
        public int PlayheadTick
        {
            get => GetValue(PlayheadTickProperty);
            set => SetValue(PlayheadTickProperty, value);
        }

        /// <summary>
        /// Ticks in one section, to place the playhead.
        /// </summary>
        public int TicksPerSection { get; set; } = 1536;

        public void Initialise([NotNull] Song arrangedSong, [NotNull] ArrangementState arrangementState)
        {
            song = arrangedSong;
            state = arrangementState;
            song.Changed += (sender, args) => Refresh();
            state.Changed += (sender, args) => InvalidateVisual();
            Refresh();
        }

        public void Refresh()
        {
            if (song != null)
            {
                Height = RulerHeight + song.Tracks.Count * RowHeight + AddRowHeight;
            }

            InvalidateVisual();
        }

        private double ColumnWidth => song == null ? 0 : Math.Max(1, (Bounds.Width - HeaderWidth) / song.SectionCount);

        /// <summary>
        /// The middle of a cell. For tests, and for placing menus.
        /// </summary>
        public Point CellCentre(int trackIndex, int section) =>
            new Point(HeaderWidth + (section + 0.5) * ColumnWidth, RulerHeight + (trackIndex + 0.5) * RowHeight);

        public Point MuteButtonCentre(int trackIndex) => new Point(HeaderWidth - 22, RulerHeight + (trackIndex + 0.5) * RowHeight);

        public Point AddTrackButtonCentre(TrackKind kind)
        {
            double top = RulerHeight + (song?.Tracks.Count ?? 0) * RowHeight;
            return new Point(AddButton(kind).Center.X, top + AddRowHeight / 2);
        }

        private Rect AddButton(TrackKind kind)
        {
            double top = RulerHeight + (song?.Tracks.Count ?? 0) * RowHeight + 7;
            return kind == TrackKind.Synth ? new Rect(12, top, 98, 22) : new Rect(116, top, 98, 22);
        }

        public override void Render([NotNull] DrawingContext context)
        {
            context.FillRectangle(Background, new Rect(Bounds.Size));

            if (song == null || state == null)
            {
                return;
            }

            double columnWidth = ColumnWidth;
            int songLength = song.LengthInSections;
            bool songMode = state.Mode == PlaybackMode.Song;

            // Section numbers, with the bar each starts on.
            for (var section = 0; section < song.SectionCount; section++)
            {
                double left = HeaderWidth + section * columnWidth;
                bool looping = !songMode && section == state.LoopSection;

                if (looping)
                {
                    context.FillRectangle(LoopColumn, new Rect(left, 0, columnWidth, Bounds.Height - AddRowHeight));
                    context.FillRectangle(Accent, new Rect(left + 2, RulerHeight - 3, columnWidth - 4, 2));
                }

                IBrush numberBrush = looping ? Accent : section < songLength ? SecondaryText : TertiaryText;
                DrawText(context, (section + 1).ToString(), new Point(left + 8, 5), 12, numberBrush, FontWeight.SemiBold);
                DrawText(context, $"bar {section * 4 + 1}", new Point(left + 24, 6), 10, TertiaryText);
            }

            for (var row = 0; row < song.Tracks.Count; row++)
            {
                Track track = song.Tracks[row];
                double top = RulerHeight + row * RowHeight;
                Color colour = TrackColours[track.ColourIndex % TrackColours.Length];
                bool selectedTrack = state.SelectedTrack == track;

                // Header: colour bar, name, kind, mute.
                context.FillRectangle(selectedTrack ? SelectedHeaderBrush : HeaderBrush, new Rect(0, top, HeaderWidth, RowHeight));
                context.FillRectangle(new SolidColorBrush(colour, track.IsMuted ? 0.35 : 1), new Rect(0, top + 6, 3, RowHeight - 12), 1.5f);
                DrawText(context, track.Name, new Point(14, top + 6), 12, track.IsMuted ? TertiaryText : PrimaryText, FontWeight.SemiBold);
                string instrument = track.Synth != null ? $"{track.Synth.Waveform} synth" : "Drum kit";
                DrawText(context, instrument, new Point(14, top + 22), 10, TertiaryText);

                var mute = new Rect(HeaderWidth - 34, top + (RowHeight - 22) / 2, 24, 22);
                context.FillRectangle(track.IsMuted ? Accent : Raised, mute, 5);
                DrawCentredText(context, "M", mute, 11, track.IsMuted ? DarkText : SecondaryText);

                for (var section = 0; section < song.SectionCount; section++)
                {
                    var cell = new Rect(HeaderWidth + section * columnWidth + 2, top + 3, columnWidth - 4, RowHeight - 6);
                    Clip clip = track.ClipAt(section);

                    if (clip == null)
                    {
                        context.FillRectangle(EmptyCellBrush, cell, 4);
                    }
                    else
                    {
                        DrawClip(context, clip, cell, colour, track.IsMuted);
                    }

                    if (selectedTrack && section == state.SelectedSection)
                    {
                        context.DrawRectangle(null, SelectionPen, cell.Inflate(1), 5, 5);
                    }
                }

                double rowBottom = Math.Floor(top + RowHeight) + 0.5;
                context.DrawLine(HairlinePen, new Point(0, rowBottom), new Point(HeaderWidth, rowBottom));
            }

            // Add-track buttons.
            foreach (TrackKind kind in new[] { TrackKind.Synth, TrackKind.Drums })
            {
                Rect button = AddButton(kind);
                bool full = song.Tracks.Count >= Song.MaximumTracks;
                context.FillRectangle(Raised, button, 5);
                DrawCentredText(context, kind == TrackKind.Synth ? "+ Synth track" : "+ Drum track", button, 11, full ? TertiaryText : SecondaryText);
            }

            DrawPlayhead(context, columnWidth, songMode);
        }

        private void DrawPlayhead([NotNull] DrawingContext context, double columnWidth, bool songMode)
        {
            if (PlayheadTick < 0 || TicksPerSection <= 0 || song == null)
            {
                return;
            }

            double sections = (double) PlayheadTick / TicksPerSection;
            double x = Math.Floor(HeaderWidth + sections * columnWidth) + 0.5;
            double bottom = RulerHeight + song.Tracks.Count * RowHeight;
            context.DrawLine(PlayheadPen, new Point(x, 0), new Point(x, bottom));
        }

        private static void DrawClip([NotNull] DrawingContext context, [NotNull] Clip clip, Rect cell, Color colour, bool muted)
        {
            context.FillRectangle(new SolidColorBrush(colour, muted ? 0.25 : 0.85), cell, 4);
            IBrush ink = new SolidColorBrush(Color.Parse("#16171A"), muted ? 0.35 : 0.55);
            DrawText(context, clip.Name, new Point(cell.X + 6, cell.Y + 3), 11, new SolidColorBrush(Color.Parse("#16171A"), muted ? 0.5 : 0.85), FontWeight.Bold);

            // A miniature of the clip, so different clips are recognisable at a glance.
            var preview = new Rect(cell.X + 6, cell.Y + 17, cell.Width - 12, cell.Height - 21);

            switch (clip)
            {
                case NoteClip notes:
                    DrawNotePreview(context, notes.Tones, preview, ink);
                    break;
                case DrumClip drums:
                    DrawDrumPreview(context, drums.Pattern, preview, ink);
                    break;
            }
        }

        private static void DrawNotePreview([NotNull] DrawingContext context, [NotNull] IReadOnlyList<Tone> tones, Rect area, [NotNull] IBrush ink)
        {
            if (tones.Count == 0 || area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            const int sectionTicks = 1536;
            List<int> pitches = tones.Select(tone => tone.Pitch.Octave * 12 + tone.Pitch.Note.Value).ToList();
            int lowest = pitches.Min();
            int range = Math.Max(1, pitches.Max() - lowest);

            for (var i = 0; i < tones.Count; i++)
            {
                double start = tones[i].StartPosition.TotalTicks(TimeSignature.FourFour, 96) / (double) sectionTicks;
                double end = tones[i].EndPosition.TotalTicks(TimeSignature.FourFour, 96) / (double) sectionTicks;
                double y = area.Bottom - (pitches[i] - lowest) / (double) range * (area.Height - 2) - 2;
                context.FillRectangle(ink, new Rect(area.X + start * area.Width, y, Math.Max(1.5, (end - start) * area.Width - 0.5), 2));
            }
        }

        private static void DrawDrumPreview([NotNull] DrawingContext context, [NotNull] DrumPattern pattern, Rect area, [NotNull] IBrush ink)
        {
            DrumSound[] sounds = DrumPattern.AllSounds;
            double rowHeight = area.Height / sounds.Length;

            for (var row = 0; row < sounds.Length; row++)
            {
                for (var step = 0; step < DrumPattern.StepCount; step++)
                {
                    if (pattern.IsHit(sounds[row], step))
                    {
                        double x = area.X + step / (double) DrumPattern.StepCount * area.Width;
                        context.FillRectangle(ink, new Rect(x, area.Y + row * rowHeight, Math.Max(1, area.Width / DrumPattern.StepCount - 0.5), Math.Max(1, rowHeight - 0.5)));
                    }
                }
            }
        }

        private static void DrawText([NotNull] DrawingContext context, [NotNull] string text, Point origin, double size, [NotNull] IBrush brush,
            FontWeight weight = FontWeight.Normal)
        {
            var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, weight);
            context.DrawText(new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, brush), origin);
        }

        private static void DrawCentredText([NotNull] DrawingContext context, [NotNull] string text, Rect area, double size, [NotNull] IBrush brush)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold), size, brush);
            context.DrawText(formatted, new Point(area.X + (area.Width - formatted.Width) / 2, area.Y + (area.Height - formatted.Height) / 2));
        }

        protected override void OnPointerPressed([NotNull] PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (song == null)
            {
                return;
            }

            Point point = e.GetPosition(this);
            PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
            bool secondary = properties.IsRightButtonPressed ||
                             (OperatingSystem.IsMacOS() && properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control));

            foreach (TrackKind kind in new[] { TrackKind.Synth, TrackKind.Drums })
            {
                if (AddButton(kind).Contains(point) && song.Tracks.Count < Song.MaximumTracks)
                {
                    AddTrackRequested?.Invoke(this, kind);
                    e.Handled = true;
                    return;
                }
            }

            var row = (int) Math.Floor((point.Y - RulerHeight) / RowHeight);

            if (point.Y < RulerHeight || row < 0 || row >= song.Tracks.Count)
            {
                return;
            }

            Track track = song.Tracks[row];

            if (point.X < HeaderWidth)
            {
                double top = RulerHeight + row * RowHeight;

                if (new Rect(HeaderWidth - 34, top, 34, RowHeight).Contains(point))
                {
                    MuteToggled?.Invoke(this, track);
                }
                else
                {
                    // Clicking a track's name edits it in the section already selected.
                    CellClicked?.Invoke(this, new CellEventArgs(track, state?.SelectedSection ?? 0, point));
                }

                e.Handled = true;
                return;
            }

            int section = Math.Clamp((int) Math.Floor((point.X - HeaderWidth) / ColumnWidth), 0, song.SectionCount - 1);
            var args = new CellEventArgs(track, section, point);

            if (secondary)
            {
                CellClicked?.Invoke(this, args);
                CellContextRequested?.Invoke(this, args);
            }
            else if (e.ClickCount >= 2)
            {
                CellDoubleClicked?.Invoke(this, args);
            }
            else
            {
                CellClicked?.Invoke(this, args);
            }

            e.Handled = true;
        }
    }

    public sealed class CellEventArgs : EventArgs
    {
        public CellEventArgs([NotNull] Track track, int section, Point point)
        {
            Track = track;
            Section = section;
            Point = point;
        }

        [NotNull]
        public Track Track { get; }

        public int Section { get; }

        public Point Point { get; }
    }
}
