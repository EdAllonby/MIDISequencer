using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// The drum kit's settings, for the side panel.
    /// </summary>
    public sealed class DrumKitViewModel : ObservableObject
    {
        [NotNull] private DrumKitSettings settings;

        public DrumKitViewModel([NotNull] DrumKitSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// The kit being edited: the selected drum track's.
        /// </summary>
        [NotNull]
        public DrumKitSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged(string.Empty);
            }
        }

        public int VolumePercent
        {
            get => (int) Math.Round(settings.Volume * 100);
            set
            {
                if (value == VolumePercent)
                {
                    return;
                }

                settings.Volume = value / 100.0;
                OnPropertyChanged(nameof(VolumePercent));
            }
        }
    }
}
