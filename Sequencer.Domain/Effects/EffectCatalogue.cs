using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Domain.Effects
{
    /// <summary>
    /// The effects you can add to a track, and their parameters. Defaults are set to sound good straight away.
    /// </summary>
    public static class EffectCatalogue
    {
        /// <summary>
        /// Delay times, in sixteenth notes, matching <see cref="DelayDivisions"/>.
        /// </summary>
        [NotNull] public static readonly IReadOnlyList<double> DelaySixteenths = new[] { 1.0, 2, 3, 4, 6, 8 };

        [NotNull] public static readonly IReadOnlyList<string> DelayDivisions = new[] { "1/16", "1/8", "1/8 dotted", "1/4", "1/4 dotted", "1/2" };

        [NotNull]
        public static IReadOnlyList<EffectKind> All { get; } = new[]
        {
            EffectKind.Compressor, EffectKind.Equaliser, EffectKind.Drive, EffectKind.Chorus, EffectKind.Delay, EffectKind.Reverb
        };

        [NotNull]
        public static string NameOf(EffectKind kind) => kind == EffectKind.Equaliser ? "EQ" : kind.ToString();

        /// <summary>
        /// One line on what the effect does, for the Add Effect menu.
        /// </summary>
        [NotNull]
        public static string DescriptionOf(EffectKind kind)
        {
            switch (kind)
            {
                case EffectKind.Compressor:
                    return "Evens out the level; makes drums punchier";
                case EffectKind.Equaliser:
                    return "Shapes the bass, middle and treble";
                case EffectKind.Drive:
                    return "Warm saturation, up to grit";
                case EffectKind.Chorus:
                    return "Thickens with a gently detuned copy";
                case EffectKind.Delay:
                    return "Echoes in time with the tempo";
                default:
                    return "Puts the track in a room or hall";
            }
        }

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<EffectParameter> ParametersOf(EffectKind kind)
        {
            switch (kind)
            {
                case EffectKind.Compressor:
                    return new[]
                    {
                        new EffectParameter("threshold", "Threshold", -40, 0, -18, ParameterUnit.Decibels),
                        new EffectParameter("ratio", "Ratio", 1, 20, 4, ParameterUnit.Ratio, logarithmic: true),
                        new EffectParameter("attack", "Attack", 0.1, 100, 10, ParameterUnit.Milliseconds, logarithmic: true),
                        new EffectParameter("release", "Release", 10, 1000, 120, ParameterUnit.Milliseconds, logarithmic: true),
                        new EffectParameter("makeup", "Makeup", 0, 24, 6, ParameterUnit.Decibels)
                    };
                case EffectKind.Equaliser:
                    return new[]
                    {
                        new EffectParameter("low", "Low", -12, 12, 0, ParameterUnit.Decibels),
                        new EffectParameter("mid", "Mid", -12, 12, 0, ParameterUnit.Decibels),
                        new EffectParameter("midFrequency", "Mid frequency", 200, 5000, 1000, ParameterUnit.Hertz, logarithmic: true),
                        new EffectParameter("high", "High", -12, 12, 0, ParameterUnit.Decibels)
                    };
                case EffectKind.Drive:
                    return new[]
                    {
                        new EffectParameter("drive", "Drive", 0, 24, 9, ParameterUnit.Decibels),
                        new EffectParameter("tone", "Tone", 500, 18000, 6000, ParameterUnit.Hertz, logarithmic: true),
                        new EffectParameter("mix", "Mix", 0, 1, 1, ParameterUnit.Percent)
                    };
                case EffectKind.Chorus:
                    return new[]
                    {
                        new EffectParameter("rate", "Rate", 0.1, 5, 0.8, ParameterUnit.Hertz, logarithmic: true),
                        new EffectParameter("depth", "Depth", 0, 1, 0.5, ParameterUnit.Percent),
                        new EffectParameter("mix", "Mix", 0, 1, 0.5, ParameterUnit.Percent)
                    };
                case EffectKind.Delay:
                    return new[]
                    {
                        new EffectParameter("time", "Time", 0, DelayDivisions.Count - 1, 2, ParameterUnit.Choice, choices: DelayDivisions),
                        new EffectParameter("feedback", "Feedback", 0, 0.9, 0.35, ParameterUnit.Percent),
                        new EffectParameter("tone", "Tone", 500, 18000, 4000, ParameterUnit.Hertz, logarithmic: true),
                        new EffectParameter("mix", "Mix", 0, 1, 0.3, ParameterUnit.Percent)
                    };
                default:
                    return new[]
                    {
                        new EffectParameter("size", "Size", 0, 1, 0.6, ParameterUnit.Percent),
                        new EffectParameter("damping", "Damping", 0, 1, 0.4, ParameterUnit.Percent),
                        new EffectParameter("mix", "Mix", 0, 1, 0.25, ParameterUnit.Percent)
                    };
            }
        }
    }
}
