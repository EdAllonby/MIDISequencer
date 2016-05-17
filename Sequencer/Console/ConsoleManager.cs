using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Sequencer.Console
{
    /// <summary>
    /// Manages a console window in a windows application.
    /// Can be used to explicitly open and close a console when project is not set to Output Type: Console Application
    /// </summary>
    public static class ConsoleManager
    {
        private static bool HasConsole => UnsafeNativeMethods.GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            if (!HasConsole)
            {
                UnsafeNativeMethods.AllocConsole();
                InvalidateOutAndError();
            }
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console
        /// is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                UnsafeNativeMethods.FreeConsole();
            }
        }

        /// <summary>
        /// Change between the two Console states.
        /// </summary>
        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            FieldInfo consoleOut = type.GetField("_out",
                BindingFlags.Static | BindingFlags.NonPublic);

            FieldInfo consoleError = type.GetField("_error",
                BindingFlags.Static | BindingFlags.NonPublic);

            MethodInfo consoleInitializeStdOutError = type.GetMethod("InitializeStdOutError",
                BindingFlags.Static | BindingFlags.NonPublic);

            Debug.Assert(consoleOut != null);
            Debug.Assert(consoleError != null);

            Debug.Assert(consoleInitializeStdOutError != null);

            consoleOut.SetValue(null, null);
            consoleError.SetValue(null, null);

            consoleInitializeStdOutError.Invoke(null, new object[] {true});
        }

        private static void SetOutAndErrorNull()
        {
            System.Console.SetOut(TextWriter.Null);
            System.Console.SetError(TextWriter.Null);
        }
    }
}