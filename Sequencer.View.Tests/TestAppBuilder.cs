using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

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
            RequestedThemeVariant = ThemeVariant.Light;
            Styles.Add(new FluentTheme());
        }
    }
}
