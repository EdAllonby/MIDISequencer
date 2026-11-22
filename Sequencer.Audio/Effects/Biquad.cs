using System;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A second-order filter section (coefficients from Robert Bristow-Johnson's Audio EQ Cookbook). Setting new
    /// coefficients keeps the filter's state, so a change while playing doesn't click.
    /// </summary>
    internal struct Biquad
    {
        private double b0, b1, b2, a1, a2;
        private double z1, z2;

        public static Biquad LowPass(double sampleRate, double cutoff, double q = 0.7071067811865476)
        {
            double w = 2 * Math.PI * cutoff / sampleRate;
            double alpha = Math.Sin(w) / (2 * q);
            double cos = Math.Cos(w);
            var filter = new Biquad();
            filter.Set((1 - cos) / 2, 1 - cos, (1 - cos) / 2, 1 + alpha, -2 * cos, 1 - alpha);
            return filter;
        }

        /// <summary>
        /// Boosts or cuts everything below <paramref name="frequency"/> (shelf slope 1).
        /// </summary>
        public void SetLowShelf(double sampleRate, double frequency, double gainDecibels) => SetShelf(sampleRate, frequency, gainDecibels, false);

        /// <summary>
        /// Boosts or cuts everything above <paramref name="frequency"/> (shelf slope 1).
        /// </summary>
        public void SetHighShelf(double sampleRate, double frequency, double gainDecibels) => SetShelf(sampleRate, frequency, gainDecibels, true);

        /// <summary>
        /// A bell around <paramref name="frequency"/>, <paramref name="octaves"/> wide.
        /// </summary>
        public void SetPeaking(double sampleRate, double frequency, double octaves, double gainDecibels)
        {
            double a = Math.Pow(10, gainDecibels / 40);
            double w = 2 * Math.PI * frequency / sampleRate;
            double sin = Math.Sin(w);
            double alpha = sin * Math.Sinh(Math.Log(2) / 2 * octaves * w / sin);
            double cos = Math.Cos(w);
            Set(1 + alpha * a, -2 * cos, 1 - alpha * a, 1 + alpha / a, -2 * cos, 1 - alpha / a);
        }

        public double Process(double input)
        {
            double output = b0 * input + z1;
            z1 = b1 * input - a1 * output + z2;
            z2 = b2 * input - a2 * output;
            return output;
        }

        private void SetShelf(double sampleRate, double frequency, double gainDecibels, bool high)
        {
            double a = Math.Pow(10, gainDecibels / 40);
            double w = 2 * Math.PI * frequency / sampleRate;
            double cos = Math.Cos(w);
            double alpha = Math.Sin(w) / 2 * Math.Sqrt(2);
            double root = 2 * Math.Sqrt(a) * alpha;
            double sign = high ? -1 : 1;

            Set(a * ((a + 1) - sign * (a - 1) * cos + root),
                sign * 2 * a * ((a - 1) - sign * (a + 1) * cos),
                a * ((a + 1) - sign * (a - 1) * cos - root),
                (a + 1) + sign * (a - 1) * cos + root,
                -sign * 2 * ((a - 1) + sign * (a + 1) * cos),
                (a + 1) + sign * (a - 1) * cos - root);
        }

        private void Set(double nb0, double nb1, double nb2, double na0, double na1, double na2)
        {
            b0 = nb0 / na0;
            b1 = nb1 / na0;
            b2 = nb2 / na0;
            a1 = na1 / na0;
            a2 = na2 / na0;
        }
    }
}
