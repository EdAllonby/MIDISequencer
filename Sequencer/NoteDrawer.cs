﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Sequencer.Domain;

namespace Sequencer
{
    public sealed class NoteDrawer
    {
        private readonly Canvas sequencer;
        private readonly SequencerSettings sequencerSettings;
        private Rectangle noteRectangle;

        public NoteDrawer(Canvas sequencer, SequencerSettings sequencerSettings)
        {
            this.sequencer = sequencer;
            this.sequencerSettings = sequencerSettings;
        }

        public void DrawNote(Pitch pitch, Position startPosition, Position endPosition, NoteState noteState,
            SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            RemoveNote();

            TimeSignature timeSignature = sequencerSettings.TimeSignature;

            double beatWidth = sequencerDimensionsCalculator.BeatWidth;
            double noteHeight = sequencerDimensionsCalculator.NoteHeight;

            double noteStartLocation = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteWidth = ActualWidthBetweenPositions(timeSignature, startPosition, endPosition, beatWidth);
            double noteStartHeight = GetPointFromPitch(pitch, sequencer.ActualHeight, noteHeight, sequencerSettings.LowestPitch);
            noteRectangle = new Rectangle
            {
                Height = noteHeight,
                Width = noteWidth,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour),
                StrokeThickness = 0.5
            };

            SetNoteColour(noteState);

            sequencer.Children.Add(noteRectangle);
            Canvas.SetLeft(noteRectangle, noteStartLocation);
            Canvas.SetTop(noteRectangle, noteStartHeight);
        }

        public void SetNoteColour(NoteState noteState)
        {
            switch (noteState)
            {
                case NoteState.Selected:
                    noteRectangle.Fill = new SolidColorBrush(sequencerSettings.SelectedNoteColour);
                    break;
                case NoteState.Unselected:
                    noteRectangle.Fill = new SolidColorBrush(sequencerSettings.UnselectedNoteColour);
                    break;
            }
        }

        public void RemoveNote()
        {
            sequencer.Children.Remove(noteRectangle);
        }

        private static double GetPointFromPitch(Pitch pitch, double sequencerHeight, double noteHeight, Pitch startingPitch)
        {
            int pitchDelta = pitch.MidiNoteNumber - startingPitch.MidiNoteNumber;
            double relativePitchPosition = (noteHeight*pitchDelta) + noteHeight;
            return sequencerHeight - relativePitchPosition;
        }

        private static double ActualWidthBetweenPositions(TimeSignature timeSignature, Position startPosition, Position endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(timeSignature, endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }

        private static double GetPointFromPosition(TimeSignature timeSignature, Position position, double beatWidth)
        {
            return (position.SummedBeat(timeSignature)*beatWidth) - beatWidth;
        }

        public bool IntersectsWith(Rect rectangle)
        {
            var noteRect = new Rect(Canvas.GetLeft(noteRectangle), Canvas.GetTop(noteRectangle), noteRectangle.Width, noteRectangle.Height);

            return rectangle.IntersectsWith(noteRect);
        }
    }
}