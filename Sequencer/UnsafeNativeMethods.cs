using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Sequencer
{
    /// <summary>
    /// Holds unsafe P/Invoke calls.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        private const string Kernel32DllName = "kernel32.dll";

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Kernel32DllName)]
        public static extern bool AllocConsole();

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get
        /// extended error information, call GetLastError.
        /// </returns>
        [DllImport(Kernel32DllName)]
        public static extern bool FreeConsole();

        /// <summary>
        /// Retrieves the window handle used by the console associated with the calling process.
        /// </summary>
        /// <returns>
        /// The return value is a handle to the window used by the console associated with the calling process or NULL if
        /// there is no such associated console.
        /// </returns>
        [DllImport(Kernel32DllName)]
        public static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// Retrieves the output code page used by the console associated with the calling process.
        /// A console uses its output code page to translate the character values written by the various output functions into the
        /// images displayed in the console window.
        /// </summary>
        /// <returns>
        /// The return value is a code that identifies the code page. For a list of identifiers, see Code Page
        /// Identifiers.
        /// </returns>
        [DllImport(Kernel32DllName)]
        public static extern int GetConsoleOutputCP();
    }
}