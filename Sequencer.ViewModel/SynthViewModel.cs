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

        [NotNull] private readonly SynthSettings settings;

        public SynthViewModel([NotNull] SynthSettings settings)
        {
            this.settings = settings;
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

        public bool PreviewOnEdit
        {
            get => settings.PreviewOnEdit;
            set => SetSetting(settings.PreviewOnEdit, value, v => settings.PreviewOnEdit = v);
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
            get => ToPercent(settings.MasterVolume);
            set => SetSetting(VolumePercent, value, v => settings.MasterVolume = v / 100.0);
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
