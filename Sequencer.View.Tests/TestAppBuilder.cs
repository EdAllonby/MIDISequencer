using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Markup.Xaml.Styling;

[assembly: AvaloniaTestApplication(typeof(Sequencer.View.Tests.TestAppBuilder))]

namespace Sequencer.View.Tests
{
    public class TestAppBuilder
    {
        // Skia rendering (not headless drawing) so frames can be captured as images.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<TestApp>()
                .UseSkia()
                .WithInterFont()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
        }
    }

    /// <summary>
    /// The real app also opens the audio device and its main window; tests build only what they need.
    /// </summary>
    public class TestApp : Application
    {
        public override void Initialize()
        {
            // The app's real theme, so rendered frames look like the app.
            // Load the app's assembly first: avares:// lookups only search assemblies already loaded.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(App).TypeHandle);
            RequestedThemeVariant = ThemeVariant.Dark;
            Styles.Add(new StyleInclude(new Uri("avares://Sequencer.View.Tests/")) { Source = new Uri("avares://Sequencer/Theme.axaml") });
        }
    }
}
