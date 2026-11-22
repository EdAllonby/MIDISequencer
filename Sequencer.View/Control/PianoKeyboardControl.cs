using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Visual;

namespace Sequencer.View.Control
{
    /// <summary>
    /// A piano keyboard alongside the editor, one key per note lane. Press a key (or drag across keys) to
    /// play it; keys light up while their notes play back.
    /// </summary>
    public sealed class PianoKeyboardControl : UserControl
    {

        [NotNull] private readonly Canvas canvas = new Canvas { Background = Brushes.Transparent };
        [NotNull] private readonly Dictionary<int, Rectangle> keys = new Dictionary<int, Rectangle>();
        [NotNull] private readonly HashSet<int> activeNotes = new HashSet<int>();

        private IDigitalAudioProtocol protocol;
        private SequencerSettings settings;
        private INotePreviewer notePreviewer;
        private IBrush whiteKeyBrush;
        private IBrush blackKeyBrush;
        private IBrush keyBorderBrush;
        private IBrush labelBrush;
        private IBrush pressedBrush;
        private IBrush playingBrush;
        private int? pressedNote;
        [CanBeNull] private IDisposable heldNote;

        public PianoKeyboardControl()
        {
            Content = canvas;
            SizeChanged += (sender, args) => Redraw();

            canvas.PointerPressed += OnPointerPressed;
            canvas.PointerMoved += OnPointerMoved;
            canvas.PointerReleased += (sender, args) => ReleaseKey(args.Pointer);
            canvas.PointerCaptureLost += (sender, args) => ReleaseKey(null);
        }

        /// <summary>
        /// MIDI note numbers of the keys lit by playback.
        /// </summary>
        [NotNull]
        public IReadOnlyCollection<int> ActiveNotes => activeNotes;

        /// <summary>
        /// The MIDI note number of the key being pressed, if any.
        /// </summary>
        public int? PressedNote => pressedNote;

        public void Initialise([NotNull] IDigitalAudioProtocol digitalAudioProtocol, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] INotePreviewer previewer)
        {
            protocol = digitalAudioProtocol;
            settings = sequencerSettings;
            notePreviewer = previewer;
            whiteKeyBrush = settings.WhiteKeyColour.ToBrush();
            blackKeyBrush = settings.BlackKeyColour.ToBrush();
            keyBorderBrush = settings.AccidentalLaneColour.ToBrush();
            labelBrush = settings.KeyLabelColour.ToBrush();
            pressedBrush = settings.ActiveKeyColour.ToBrush();
            playingBrush = new SolidColorBrush(settings.ActiveKeyColour.ToAvalonia(), 0.7);
            Redraw();
        }

        /// <summary>
        /// Lights the keys for these pitches and clears the rest.
        /// </summary>
        public void SetActivePitches([NotNull] IEnumerable<Pitch> pitches)
        {
            if (protocol == null)
            {
                return;
            }

            activeNotes.Clear();
            activeNotes.UnionWith(pitches.Select(protocol.ProtocolNoteNumber));
            RefreshKeyColours();
        }

        private int LowestNote => protocol.ProtocolNoteNumber(settings.LowestPitch);

        private double LaneHeight => canvas.Bounds.Height / settings.TotalNotes;

        private void Redraw()
        {
            canvas.Children.Clear();
            keys.Clear();

            if (protocol == null || canvas.Bounds.Height <= 0)
            {
                return;
            }

            double width = canvas.Bounds.Width;

            // Lane 0 is the top of the editor, which is the highest note.
            for (var lane = 0; lane < settings.TotalNotes; lane++)
            {
                int noteNumber = LowestNote + settings.TotalNotes - 1 - lane;
                Pitch pitch = protocol.CreatePitchFromProtocolNumber(noteNumber);
                bool isBlack = pitch.Note.IsAccidental;
                double top = lane * LaneHeight;

                var background = new Rectangle { Width = width, Height = LaneHeight, Fill = whiteKeyBrush, Stroke = keyBorderBrush, StrokeThickness = 0.5 };
                Canvas.SetTop(background, top);
                canvas.Children.Add(background);

                // Black keys are shorter, as on a piano.
                var key = new Rectangle
                {
                    Width = isBlack ? width * 0.6 : width,
                    Height = LaneHeight,
                    Fill = isBlack ? blackKeyBrush : whiteKeyBrush,
                    Stroke = keyBorderBrush,
                    StrokeThickness = 0.5,
                    RadiusX = isBlack ? 1.5 : 0,
                    RadiusY = isBlack ? 1.5 : 0
                };
                Canvas.SetTop(key, top);
                canvas.Children.Add(key);
                keys[noteNumber] = key;

                if (pitch.Note == Note.C)
                {
                    var label = new TextBlock
                    {
                        Text = pitch.ToString(),
                        FontSize = 9,
                        Foreground = labelBrush,
                        FontWeight = FontWeight.SemiBold,
                        Width = width - 4,
                        TextAlignment = TextAlignment.Right,
                        IsHitTestVisible = false
                    };
                    Canvas.SetTop(label, top + (LaneHeight - 12) / 2);
                    canvas.Children.Add(label);
                }
            }

            RefreshKeyColours();
        }

        private void RefreshKeyColours()
        {
            foreach (KeyValuePair<int, Rectangle> key in keys)
            {
                key.Value.Fill = KeyBrush(key.Key);
            }
        }

        [NotNull]
        private IBrush KeyBrush(int noteNumber)
        {
            if (noteNumber == pressedNote)
            {
                return pressedBrush;
            }

            if (activeNotes.Contains(noteNumber))
            {
                return playingBrush;
            }

            return protocol.CreatePitchFromProtocolNumber(noteNumber).Note.IsAccidental ? blackKeyBrush : whiteKeyBrush;
        }

        private int? NoteAtPoint(Point point)
        {
            if (protocol == null || LaneHeight <= 0)
            {
                return null;
            }

            var lane = (int) Math.Floor(point.Y / LaneHeight);

            if (lane < 0 || lane >= settings.TotalNotes)
            {
                return null;
            }

            return LowestNote + settings.TotalNotes - 1 - lane;
        }

        private void OnPointerPressed(object sender, [NotNull] PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
            {
                return;
            }

            e.Pointer.Capture(canvas);
            PressKey(NoteAtPoint(e.GetPosition(canvas)));
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, [NotNull] PointerEventArgs e)
        {
            if (pressedNote == null)
            {
                return;
            }

            // Sliding across the keys plays each one in turn.
            int? note = NoteAtPoint(e.GetPosition(canvas));

            if (note != null && note != pressedNote)
            {
                PressKey(note);
            }
        }

        private void PressKey(int? noteNumber)
        {
            heldNote?.Dispose();
            heldNote = null;
            pressedNote = noteNumber;

            if (noteNumber != null)
            {
                Pitch pitch = protocol.CreatePitchFromProtocolNumber(noteNumber.Value);
                var start = new Position(1, 1, 1);
                heldNote = notePreviewer.Hold(new Tone(pitch, settings.DefaultVelocity, start, start));
            }

            RefreshKeyColours();
        }

        private void ReleaseKey([CanBeNull] IPointer pointer)
        {
            if (pressedNote == null && heldNote == null)
            {
                return;
            }

            PressKey(null);
            pointer?.Capture(null);
        }
    }
}
