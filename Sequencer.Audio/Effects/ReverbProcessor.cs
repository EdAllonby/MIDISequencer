using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A room reverb (the Schroeder–Moorer design popularised as Freeverb): eight damped feedback combs in
    /// parallel build the dense, decaying tail; four all-pass filters in series diffuse it. Size sets how long the
    /// tail rings, damping how quickly its treble fades.
    /// </summary>
    public sealed class ReverbProcessor : EffectProcessor
    {
        // Freeverb's tunings, in samples at 44.1 kHz; scaled for other rates.
        private static readonly int[] CombTunings = { 1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617 };
        private static readonly int[] AllPassTunings = { 556, 441, 341, 225 };
        private const double InputGain = 0.015;

        [NotNull] private readonly Comb[] combs;
        [NotNull] private readonly AllPass[] allPasses;
        private Smoothed mix;
        private Smoothed size;
        private Smoothed damping;

        public ReverbProcessor([NotNull] Effect effect, int sampleRate) : base(effect, sampleRate)
        {
            double scale = sampleRate / 44100.0;
            combs = Array.ConvertAll(CombTunings, tuning => new Comb((int) (tuning * scale)));
            allPasses = Array.ConvertAll(AllPassTunings, tuning => new AllPass((int) (tuning * scale)));
            mix = new Smoothed(sampleRate);
            size = new Smoothed(sampleRate, 0.1);
            damping = new Smoothed(sampleRate, 0.1);
        }

        public override void Process(Span<float> buffer)
        {
            // Size 0..1 → comb feedback 0.7..0.98 (a short room to a long hall).
            size.Target = 0.7 + Effect["size"] * 0.28;
            damping.Target = Effect["damping"] * 0.4;
            mix.Target = Effect["mix"];

            for (var n = 0; n < buffer.Length; n++)
            {
                float dry = buffer[n];
                double input = dry * InputGain;
                double roomFeedback = size.Next();
                double damp = damping.Next();
                double wet = 0;

                for (var c = 0; c < combs.Length; c++)
                {
                    wet += combs[c].Process(input, roomFeedback, damp);
                }

                for (var a = 0; a < allPasses.Length; a++)
                {
                    wet = allPasses[a].Process(wet);
                }

                // Like a send: the dry sound stays, the room is added underneath it.
                buffer[n] = (float) (dry + wet * mix.Next() * 3);
            }
        }

        private sealed class Comb
        {
            [NotNull] private readonly double[] line;
            private int position;
            private double filtered;

            public Comb(int length)
            {
                line = new double[Math.Max(1, length)];
            }

            public double Process(double input, double feedback, double damping)
            {
                double output = line[position];
                filtered = output * (1 - damping) + filtered * damping;
                line[position] = input + filtered * feedback;
                position = position + 1 == line.Length ? 0 : position + 1;
                return output;
            }
        }

        private sealed class AllPass
        {
            private const double Feedback = 0.5;
            [NotNull] private readonly double[] line;
            private int position;

            public AllPass(int length)
            {
                line = new double[Math.Max(1, length)];
            }

            public double Process(double input)
            {
                double delayed = line[position];
                line[position] = input + delayed * Feedback;
                position = position + 1 == line.Length ? 0 : position + 1;
                return delayed - input;
            }
        }
    }
}
