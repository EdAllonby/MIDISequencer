using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Sequencer.Domain.Effects
{
    public enum EffectKind
    {
        Compressor,
        Equaliser,
        Drive,
        Chorus,
        Delay,
        Reverb
    }

    /// <summary>
    /// How a parameter's value reads and moves.
    /// </summary>
    public enum ParameterUnit
    {
        Percent,
        Decibels,
        Milliseconds,
        Hertz,
        Ratio,
        Choice
    }

    /// <summary>
    /// One knob of an effect: its range and how to show it. The value lives on the <see cref="Effect"/>.
    /// </summary>
    public sealed class EffectParameter
    {
        public EffectParameter([NotNull] string key, [NotNull] string name, double minimum, double maximum, double defaultValue, ParameterUnit unit,
            bool logarithmic = false, [CanBeNull] [ItemNotNull] IReadOnlyList<string> choices = null)
        {
            Key = key;
            Name = name;
            Minimum = minimum;
            Maximum = maximum;
            Default = defaultValue;
            Unit = unit;
            IsLogarithmic = logarithmic;
            Choices = choices;
        }

        /// <summary>
        /// The name in saved files.
        /// </summary>
        [NotNull]
        public string Key { get; }

        [NotNull]
        public string Name { get; }

        public double Minimum { get; }

        public double Maximum { get; }

        public double Default { get; }

        public ParameterUnit Unit { get; }

        /// <summary>
        /// True for times and frequencies, which a slider should move through by ratio rather than by step.
        /// </summary>
        public bool IsLogarithmic { get; }

        /// <summary>
        /// The labels of a <see cref="ParameterUnit.Choice"/> parameter, whose value is the index.
        /// </summary>
        [CanBeNull]
        [ItemNotNull]
        public IReadOnlyList<string> Choices { get; }

        public double Clamp(double value)
        {
            if (double.IsNaN(value))
            {
                return Default;
            }

            value = Math.Clamp(value, Minimum, Maximum);
            return Unit == ParameterUnit.Choice ? Math.Round(value) : value;
        }

        /// <summary>
        /// The value as it should read: "-18 dB", "4:1", "120 ms", "1.2 kHz", "35%", "1/8 dotted".
        /// </summary>
        [NotNull]
        public string Format(double value)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            switch (Unit)
            {
                case ParameterUnit.Percent:
                    return $"{Math.Round(value * 100).ToString(culture)} %";
                case ParameterUnit.Decibels:
                    double decibels = Math.Round(value, 1);
                    return $"{(decibels > 0 ? "+" : decibels < 0 ? "−" : string.Empty)}{Math.Abs(decibels).ToString("0.#", culture)} dB";
                case ParameterUnit.Milliseconds:
                    return value < 10 ? $"{value.ToString("0.0", culture)} ms" : $"{Math.Round(value).ToString(culture)} ms";
                case ParameterUnit.Hertz:
                    return value >= 1000 ? $"{(value / 1000).ToString("0.0#", culture)} kHz" : value < 10 ? $"{value.ToString("0.0#", culture)} Hz" : $"{Math.Round(value).ToString(culture)} Hz";
                case ParameterUnit.Ratio:
                    return value >= 19.95 ? "∞:1" : $"{value.ToString("0.#", culture)}:1";
                case ParameterUnit.Choice:
                    return Choices?[(int) Clamp(value)] ?? value.ToString(culture);
                default:
                    return value.ToString(culture);
            }
        }

        /// <summary>
        /// Where a value sits along a slider, 0 to 1.
        /// </summary>
        public double ToPosition(double value)
        {
            value = Clamp(value);

            return IsLogarithmic
                ? Math.Log(value / Minimum) / Math.Log(Maximum / Minimum)
                : (value - Minimum) / (Maximum - Minimum);
        }

        public double FromPosition(double position)
        {
            position = Math.Clamp(position, 0, 1);

            return Clamp(IsLogarithmic
                ? Minimum * Math.Pow(Maximum / Minimum, position)
                : Minimum + (Maximum - Minimum) * position);
        }
    }

    /// <summary>
    /// An effect on a track: its kind, whether it's on, and its parameter values. Values can be read from the
    /// audio thread while the UI changes them.
    /// </summary>
    public sealed class Effect
    {
        [NotNull] private readonly double[] values;
        private volatile bool isEnabled = true;

        public Effect(EffectKind kind)
        {
            Kind = kind;
            Parameters = EffectCatalogue.ParametersOf(kind);
            values = Parameters.Select(parameter => parameter.Default).ToArray();
        }

        public EffectKind Kind { get; }

        [NotNull]
        public string Name => EffectCatalogue.NameOf(Kind);

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<EffectParameter> Parameters { get; }

        /// <summary>
        /// Off means bypassed: the sound passes through untouched, but the settings are kept.
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double this[[NotNull] string key]
        {
            get => Volatile.Read(ref values[IndexOf(key)]);
            set
            {
                int index = IndexOf(key);
                double clamped = Parameters[index].Clamp(value);

                if (Volatile.Read(ref values[index]) != clamped)
                {
                    Volatile.Write(ref values[index], clamped);
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler Changed;

        [NotNull]
        public Effect Clone()
        {
            var copy = new Effect(Kind) { isEnabled = isEnabled };
            Array.Copy(values, copy.values, values.Length);
            return copy;
        }

        /// <summary>
        /// Whether the effect has a parameter: files may name ones this version doesn't know.
        /// </summary>
        public bool Has([NotNull] string key) => Parameters.Any(parameter => parameter.Key == key);

        private int IndexOf([NotNull] string key)
        {
            for (var i = 0; i < Parameters.Count; i++)
            {
                if (Parameters[i].Key == key)
                {
                    return i;
                }
            }

            throw new ArgumentException($"{Name} has no parameter \"{key}\".", nameof(key));
        }

        public override string ToString() => Name;
    }
}
