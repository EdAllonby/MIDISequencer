using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Utilities;
using Sequencer.Visual;

namespace Sequencer.View.Drawing
{
    /// <summary>
    /// Draws the editor's background: note lanes and the grid.
    /// </summary>
    public sealed class SequencerDrawer
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerDrawer));
        [NotNull] [ItemNotNull] private readonly List<Avalonia.Controls.Control> elementCache = new List<Avalonia.Controls.Control>();
        [NotNull] private readonly IDigitalAudioProtocol protocol;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvas;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerDrawer([NotNull] IDigitalAudioProtocol protocol, [NotNull] ISequencerCanvasWrapper sequencerCanvas, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] SequencerSettings sequencerSettings)
        {
            this.protocol = protocol;
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerNotes = sequencerNotes;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerSettings = sequencerSettings;
        }

        /// <summary>
        /// Centres a 1 px line on a device pixel so it renders crisp rather than blurred across two.
        /// </summary>
        public static double Snap(double position) => Math.Floor(position) + 0.5;

        public void RedrawEditor()
        {
            RemoveElements();
            DrawLanes();
            DrawVerticalLines();
            AddElements();
            sequencerNotes.DrawNotes();
            Log.Debug("Sequencer redrawn");
        }

        private void DrawLanes()
        {
            double laneHeight = sequencerDimensionsCalculator.NoteHeight;
            double width = sequencerCanvas.Width;
            IBrush lane = sequencerSettings.LaneColour.ToBrush();
            IBrush accidentalLane = sequencerSettings.AccidentalLaneColour.ToBrush();
            IBrush separator = sequencerSettings.SixteenthLineColour.ToBrush();
            IBrush octave = sequencerSettings.OctaveLineColour.ToBrush();

            // Lane 0 is the top of the editor, which is the highest note.
            for (var laneIndex = 0; laneIndex < sequencerSettings.TotalNotes; laneIndex++)
            {
                int noteNumber = protocol.ProtocolNoteNumber(sequencerSettings.LowestPitch) + sequencerSettings.TotalNotes - 1 - laneIndex;
                Pitch pitch = protocol.CreatePitchFromProtocolNumber(noteNumber);
                double top = laneHeight * laneIndex;

                var background = new Rectangle { Width = width, Height = laneHeight, Fill = pitch.Note.IsAccidental ? accidentalLane : lane };
                Avalonia.Controls.Canvas.SetTop(background, top);
                elementCache.Add(background);

                // The bottom edge of a C lane is an octave boundary.
                double bottom = Snap(top + laneHeight);
                elementCache.Add(HorizontalLine(bottom, width, pitch.Note == Note.C ? octave : separator));
            }
        }

        private void DrawVerticalLines()
        {
            double height = sequencerCanvas.Height;
            IBrush sixteenth = sequencerSettings.SixteenthLineColour.ToBrush();
            IBrush beat = sequencerSettings.BeatLineColour.ToBrush();
            IBrush bar = sequencerSettings.BarLineColour.ToBrush();
            int sixteenthsPerBeat = 4;
            int sixteenthsPerBar = sixteenthsPerBeat * sequencerSettings.TimeSignature.BeatsPerBar;
            int totalSixteenths = sixteenthsPerBar * sequencerSettings.TimeSignature.BarsPerMeasure * sequencerSettings.TotalMeasures;

            for (var step = 0; step <= totalSixteenths; step++)
            {
                double x = Snap(Math.Min(step * sequencerDimensionsCalculator.SixteenthNoteWidth, sequencerCanvas.Width - 1));
                IBrush brush = step % sixteenthsPerBar == 0 ? bar : step % sixteenthsPerBeat == 0 ? beat : sixteenth;
                elementCache.Add(new Line { StartPoint = new Point(x, 0), EndPoint = new Point(x, height), StrokeThickness = 1, Stroke = brush });
            }
        }

        [NotNull]
        private static Line HorizontalLine(double y, double width, [NotNull] IBrush brush)
        {
            return new Line { StartPoint = new Point(0, y), EndPoint = new Point(width, y), StrokeThickness = 1, Stroke = brush };
        }

        private void AddElements()
        {
            foreach (Avalonia.Controls.Control element in elementCache)
            {
                sequencerCanvas.AddChild(element);
            }
        }

        private void RemoveElements()
        {
            foreach (Avalonia.Controls.Control element in elementCache)
            {
                sequencerCanvas.RemoveChild(element);
            }

            elementCache.Clear();
        }
    }
}
