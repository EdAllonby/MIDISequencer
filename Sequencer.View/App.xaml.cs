﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using JetBrains.Annotations;
using log4net.Config;
using Microsoft.Practices.ServiceLocation;
using Sequencer.Audio;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.View.Console;
using Sequencer.ViewModel;
using Sequencer.Visual;

// ReSharper disable once RedundantUsingDirective

namespace Sequencer.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static void SetupLogging([NotNull] string logConfigName)
        {
            string assemblyPath = Assembly.GetAssembly(typeof(App))?.Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);

            if (assemblyDirectory != null)
            {
                var uri = new Uri(Path.Combine(assemblyDirectory, logConfigName));

                XmlConfigurator.Configure(uri);
            }
        }

        private static void RegisterServices()
        {
            var builder = new ContainerBuilder();

            try
            {
                // ReSharper disable PossibleNullReferenceException
                builder.RegisterType<WpfDispatcher>().As<IWpfDispatcher>().SingleInstance();
                builder.RegisterType<SequencerSettings>().As<IColourSettings, IMusicalSettings, IAudioSettings>().SingleInstance();
                builder.RegisterType<TickCalculator>().As<ITickCalculator>().SingleInstance();
                builder.RegisterType<InternalClock>().As<IInternalClock>().SingleInstance();
                builder.RegisterType<SequencerClock>().As<ISequencerClock>().SingleInstance();
                builder.RegisterType<PitchAndPositionCalculator>().As<IPitchAndPositionCalculator>().SingleInstance();
                builder.RegisterType<FrequencyCalculator>().As<IFrequencyCalculator>().SingleInstance();
                builder.RegisterType<SignalProviderFactory>().As<ISignalProviderFactory>().SingleInstance();
                builder.RegisterType<SequencerNotes>().As<ISequencerNotes>().SingleInstance();
                builder.RegisterType<SequencerPlayer>().SingleInstance();
                builder.RegisterType<SequencerViewModel>().SingleInstance();
                // ReSharper restore PossibleNullReferenceException
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }

            IContainer container = builder.Build();
            container.Resolve<SequencerPlayer>();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
        }

        /// <summary>
        /// When the application has started, give it an appropriate logging mechanism, and show the log in view.
        /// </summary>
        /// <param name="e">Startup event args.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            // Console must be started before configuring log4net.
            ConsoleWindow.Show();
            SetupLogging("log4netDebug.config");
#else
            SetupLogging("log4netRelease.config");
#endif
            Thread.CurrentThread.Name = "Main Thread";

            RegisterServices();
            base.OnStartup(e);
        }
    }
}