using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// The synth panel: waveform, envelope and volume, in units for sliders (milliseconds and percent).
    /// Changes to the waveform and envelope apply to the next note played; volume applies immediately.
    /// </summary>
    public sealed class SynthViewModel : ObservableObject
    {
        public const int MaximumEnvelopeMilliseconds = 2000;

        [NotNull] private SynthSettings settings;

        public SynthViewModel([NotNull] SynthSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// The synth being edited: the selected track's.
        /// </summary>
        [NotNull]
        public SynthSettings Settings
        {
            get => settings;
            set
            {
                settings = value;

                // Every property now reads from a different synth.
                OnPropertyChanged(string.Empty);
            }
        }

        [NotNull]
        public IReadOnlyList<Waveform> Waveforms { get; } = (Waveform[]) Enum.GetValues(typeof(Waveform));

        public Waveform Waveform
        {
            get => settings.Waveform;
            set
            {
                SetSetting(settings.Waveform, value, v => settings.Waveform = v);
                OnPropertyChanged(nameof(IsSawtooth));
                OnPropertyChanged(nameof(IsSquare));
                OnPropertyChanged(nameof(IsTriangle));
                OnPropertyChanged(nameof(IsSine));
            }
        }

        // One toggle per waveform, for a segmented control.
        public bool IsSawtooth
        {
            get => Waveform == Waveform.Sawtooth;
            set => SelectWaveformIf(value, Waveform.Sawtooth);
        }

        public bool IsSquare
        {
            get => Waveform == Waveform.Square;
            set => SelectWaveformIf(value, Waveform.Square);
        }

        public bool IsTriangle
        {
            get => Waveform == Waveform.Triangle;
            set => SelectWaveformIf(value, Waveform.Triangle);
        }

        public bool IsSine
        {
            get => Waveform == Waveform.Sine;
            set => SelectWaveformIf(value, Waveform.Sine);
        }

        public int AttackMilliseconds
        {
            get => ToMilliseconds(settings.AttackSeconds);
            set => SetSetting(AttackMilliseconds, value, v => settings.AttackSeconds = v / 1000.0);
        }

        public int DecayMilliseconds
        {
            get => ToMilliseconds(settings.DecaySeconds);
            set => SetSetting(DecayMilliseconds, value, v => settings.DecaySeconds = v / 1000.0);
        }

        public int SustainPercent
        {
            get => ToPercent(settings.SustainLevel);
            set => SetSetting(SustainPercent, value, v => settings.SustainLevel = v / 100.0);
        }

        public int ReleaseMilliseconds
        {
            get => ToMilliseconds(settings.ReleaseSeconds);
            set => SetSetting(ReleaseMilliseconds, value, v => settings.ReleaseSeconds = v / 1000.0);
        }

        public int VolumePercent
        {
            get => ToPercent(settings.Volume);
            set => SetSetting(VolumePercent, value, v => settings.Volume = v / 100.0);
        }

        private void SelectWaveformIf(bool selected, Waveform waveform)
        {
            if (selected)
            {
                Waveform = waveform;
            }
            else
            {
                // One waveform is always selected: unchecking the current one re-checks it.
                OnPropertyChanged(nameof(IsSawtooth));
                OnPropertyChanged(nameof(IsSquare));
                OnPropertyChanged(nameof(IsTriangle));
                OnPropertyChanged(nameof(IsSine));
            }
        }

        // Oscillator 2.
        public bool IsOscillator2Sawtooth { get => settings.Oscillator2Waveform == Waveform.Sawtooth; set => SelectOscillator2If(value, Waveform.Sawtooth); }

        public bool IsOscillator2Square { get => settings.Oscillator2Waveform == Waveform.Square; set => SelectOscillator2If(value, Waveform.Square); }

        public bool IsOscillator2Triangle { get => settings.Oscillator2Waveform == Waveform.Triangle; set => SelectOscillator2If(value, Waveform.Triangle); }

        public bool IsOscillator2Sine { get => settings.Oscillator2Waveform == Waveform.Sine; set => SelectOscillator2If(value, Waveform.Sine); }

        public int Oscillator2Semitones
        {
            get => settings.Oscillator2Semitones;
            set => SetSetting(Oscillator2Semitones, value, v => settings.Oscillator2Semitones = v);
        }

        public int Oscillator2DetuneCents
        {
            get => (int) Math.Round(settings.Oscillator2Detune);
            set => SetSetting(Oscillator2DetuneCents, value, v => settings.Oscillator2Detune = v);
        }

        /// <summary>
        /// 0: only oscillator 1; 100: only oscillator 2.
        /// </summary>
        public int OscillatorMixPercent
        {
            get => ToPercent(settings.OscillatorMix);
            set
            {
                SetSetting(OscillatorMixPercent, value, v => settings.OscillatorMix = v / 100.0);
                OnPropertyChanged(nameof(OscillatorMixText));
            }
        }

        /// <summary>
        /// The balance of the two oscillators, as "osc 1 / osc 2" percentages.
        /// </summary>
        [NotNull]
        public string OscillatorMixText => $"{100 - OscillatorMixPercent} / {OscillatorMixPercent}";

        public int NoisePercent
        {
            get => ToPercent(settings.NoiseLevel);
            set => SetSetting(NoisePercent, value, v => settings.NoiseLevel = v / 100.0);
        }

        // Filter.
        public bool IsLowPass { get => settings.FilterType == FilterType.LowPass; set => SelectFilterIf(value, FilterType.LowPass); }

        public bool IsHighPass { get => settings.FilterType == FilterType.HighPass; set => SelectFilterIf(value, FilterType.HighPass); }

        public bool IsBandPass { get => settings.FilterType == FilterType.BandPass; set => SelectFilterIf(value, FilterType.BandPass); }

        /// <summary>
        /// The cutoff on a logarithmic scale from 0 (20 Hz) to 1000 (18 kHz), so each octave gets equal travel.
        /// </summary>
        public int CutoffPosition
        {
            get => (int) Math.Round(Math.Log(settings.Cutoff / SynthSettings.MinimumCutoff) / CutoffRange * CutoffSteps);
            set
            {
                if (value == CutoffPosition)
                {
                    return;
                }

                settings.Cutoff = SynthSettings.MinimumCutoff * Math.Exp(Math.Clamp(value, 0, CutoffSteps) / (double) CutoffSteps * CutoffRange);
                OnPropertyChanged(nameof(CutoffPosition));
                OnPropertyChanged(nameof(CutoffText));
            }
        }

        [NotNull]
        public string CutoffText => settings.Cutoff >= 1000 ? $"{settings.Cutoff / 1000:0.0} kHz" : $"{settings.Cutoff:0} Hz";

        public int ResonancePercent
        {
            get => ToPercent(settings.Resonance);
            set => SetSetting(ResonancePercent, value, v => settings.Resonance = v / 100.0);
        }

        /// <summary>
        /// In tenths of an octave, from -50 to +50.
        /// </summary>
        public int FilterEnvelopeAmountTenths
        {
            get => (int) Math.Round(settings.FilterEnvelopeAmount * 10);
            set
            {
                SetSetting(FilterEnvelopeAmountTenths, value, v => settings.FilterEnvelopeAmount = v / 10.0);
                OnPropertyChanged(nameof(FilterEnvelopeAmountText));
            }
        }

        [NotNull]
        public string FilterEnvelopeAmountText => $"{settings.FilterEnvelopeAmount:+0.0;-0.0;0.0} oct";

        public int KeyTrackingPercent
        {
            get => ToPercent(settings.KeyTracking);
            set => SetSetting(KeyTrackingPercent, value, v => settings.KeyTracking = v / 100.0);
        }

        public int FilterAttackMilliseconds
        {
            get => ToMilliseconds(settings.FilterAttackSeconds);
            set => SetSetting(FilterAttackMilliseconds, value, v => settings.FilterAttackSeconds = v / 1000.0);
        }

        public int FilterDecayMilliseconds
        {
            get => ToMilliseconds(settings.FilterDecaySeconds);
            set => SetSetting(FilterDecayMilliseconds, value, v => settings.FilterDecaySeconds = v / 1000.0);
        }

        public int FilterSustainPercent
        {
            get => ToPercent(settings.FilterSustainLevel);
            set => SetSetting(FilterSustainPercent, value, v => settings.FilterSustainLevel = v / 100.0);
        }

        public int FilterReleaseMilliseconds
        {
            get => ToMilliseconds(settings.FilterReleaseSeconds);
            set => SetSetting(FilterReleaseMilliseconds, value, v => settings.FilterReleaseSeconds = v / 1000.0);
        }

        // LFO.

        /// <summary>
        /// In tenths of a hertz, from 1 (0.1 Hz) to 200 (20 Hz).
        /// </summary>
        public int LfoRateTenths
        {
            get => (int) Math.Round(settings.LfoRate * 10);
            set
            {
                SetSetting(LfoRateTenths, value, v => settings.LfoRate = v / 10.0);
                OnPropertyChanged(nameof(LfoRateText));
            }
        }

        [NotNull]
        public string LfoRateText => $"{settings.LfoRate:0.0} Hz";

        public int LfoDepthPercent
        {
            get => ToPercent(settings.LfoDepth);
            set => SetSetting(LfoDepthPercent, value, v => settings.LfoDepth = v / 100.0);
        }

        public bool IsLfoOnPitch { get => settings.LfoTarget == LfoTarget.Pitch; set => SelectLfoTargetIf(value, LfoTarget.Pitch); }

        public bool IsLfoOnFilter { get => settings.LfoTarget == LfoTarget.Filter; set => SelectLfoTargetIf(value, LfoTarget.Filter); }

        public bool IsLfoOnVolume { get => settings.LfoTarget == LfoTarget.Volume; set => SelectLfoTargetIf(value, LfoTarget.Volume); }

        // Presets.

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<SynthPreset> Presets => SynthPresets.All;

        /// <summary>
        /// Replaces the whole patch with a preset's.
        /// </summary>
        public void ApplyPreset([NotNull] SynthPreset preset)
        {
            settings.CopyFrom(preset.CreatePatch());
            OnPropertyChanged(string.Empty);
        }

        private const int CutoffSteps = 1000;

        private static readonly double CutoffRange = Math.Log(SynthSettings.MaximumCutoff / SynthSettings.MinimumCutoff);

        private void SelectOscillator2If(bool selected, Waveform waveform)
        {
            if (selected)
            {
                settings.Oscillator2Waveform = waveform;
            }

            OnPropertyChanged(nameof(IsOscillator2Sawtooth));
            OnPropertyChanged(nameof(IsOscillator2Square));
            OnPropertyChanged(nameof(IsOscillator2Triangle));
            OnPropertyChanged(nameof(IsOscillator2Sine));
        }

        private void SelectFilterIf(bool selected, FilterType type)
        {
            if (selected)
            {
                settings.FilterType = type;
            }

            OnPropertyChanged(nameof(IsLowPass));
            OnPropertyChanged(nameof(IsHighPass));
            OnPropertyChanged(nameof(IsBandPass));
        }

        private void SelectLfoTargetIf(bool selected, LfoTarget target)
        {
            if (selected)
            {
                settings.LfoTarget = target;
            }

            OnPropertyChanged(nameof(IsLfoOnPitch));
            OnPropertyChanged(nameof(IsLfoOnFilter));
            OnPropertyChanged(nameof(IsLfoOnVolume));
        }

        private static int ToMilliseconds(double seconds) => (int) Math.Round(seconds * 1000);

        private static int ToPercent(double fraction) => (int) Math.Round(fraction * 100);

        private void SetSetting<T>(T current, T value, [NotNull] Action<T> apply, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(current, value))
            {
                return;
            }

            // The settings clamp out-of-range values, so always re-read through the getter.
            apply(value);
            OnPropertyChanged(propertyName);
        }
    }
}
