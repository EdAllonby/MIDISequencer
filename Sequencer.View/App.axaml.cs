using System;
using System.IO;
using System.Threading;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using log4net;
using log4net.Config;
using Sequencer.Audio;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.View.Control;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View
{
    public class App : Application
    {
        private IContainer container;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ConfigureLogging();
            Thread.CurrentThread.Name = "UI";

            container = BuildContainer();

            // Resolving the player opens the audio output and starts it listening to the clock.
            container.Resolve<SequencerPlayer>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = container.Resolve<MainWindow>();
                desktop.ShutdownRequested += (sender, args) =>
                {
                    if (container.Resolve<IAudioOutput>() is PortAudioOutput output)
                    {
                        // Underflows are dropouts. Clipped counts samples beyond ±1 after the limiter, so it should stay 0.
                        LogManager.GetLogger(typeof(App)).Info($"Audio: {output.Statistics}");
                    }

                    container.Dispose();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <param name="overrides">Registrations that replace the defaults, for tests.</param>
        internal static IContainer BuildContainer(Action<ContainerBuilder> overrides = null)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AvaloniaDispatcher>().As<IUiDispatcher>().SingleInstance();
            builder.RegisterType<SequencerSettings>().AsSelf().As<IColourSettings, IMusicalSettings>().SingleInstance();
            builder.RegisterType<SynthSettings>().AsSelf().As<IAudioSettings>().SingleInstance();
            builder.RegisterType<TickCalculator>().As<ITickCalculator>().SingleInstance();
            builder.RegisterType<InternalClock>().As<IInternalClock>().SingleInstance();
            builder.RegisterType<SequencerClock>().As<ISequencerClock>().SingleInstance();
            builder.RegisterType<PitchAndPositionCalculator>().As<IPitchAndPositionCalculator>().SingleInstance();
            builder.RegisterType<MidiProtocol>().As<IDigitalAudioProtocol>().SingleInstance();
            builder.RegisterType<FrequencyCalculator>().As<IFrequencyCalculator>().SingleInstance();
            builder.RegisterType<SignalProviderFactory>().As<ISignalProviderFactory>().SingleInstance();
            builder.RegisterType<PortAudioOutput>().As<IAudioOutput>().SingleInstance();
            builder.RegisterType<SequencerNotes>().As<ISequencerNotes, ISequencedNotes>().SingleInstance();
            builder.RegisterType<SequencerPlayer>().SingleInstance();
            builder.RegisterType<Sequencer.Domain.Drums.DrumPattern>().SingleInstance();
            builder.RegisterType<Sequencer.Domain.Drums.DrumKitSettings>().SingleInstance();
            builder.RegisterType<NotePreviewer>().As<INotePreviewer>().SingleInstance();
            builder.RegisterType<KeyboardStateProcessor>().AsSelf().As<IKeyboardStateProcessor>().SingleInstance();
            builder.RegisterType<MouseOperator>().AsSelf().As<IMouseOperator>().SingleInstance();
            builder.RegisterType<SequencerViewModel>().SingleInstance();
            builder.RegisterType<SynthViewModel>().SingleInstance();
            builder.RegisterType<DrumKitViewModel>().SingleInstance();
            builder.RegisterType<MainWindow>().SingleInstance();

            overrides?.Invoke(builder);

            return builder.Build();
        }

        private static void ConfigureLogging()
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "log4net.config");

            if (File.Exists(configPath))
            {
                XmlConfigurator.Configure(LogManager.GetRepository(typeof(App).Assembly), new FileInfo(configPath));
            }
        }
    }
}
