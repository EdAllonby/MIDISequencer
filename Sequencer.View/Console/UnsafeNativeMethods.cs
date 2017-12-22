using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Sequencer.View.Console
{
    /// <summary>
    /// Holds unsafe P/Invoke calls.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class UnsafeNativeMethods
    {
        private const string Kernel32DllName = "kernel32.dll";

        private const string User32DllName = "user32.dll";

        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        public const int MY_CODE_PAGE = 437;


        [DllImport(Kernel32DllName)]
        public static extern bool SetConsoleScreenBufferSize(
            IntPtr hConsoleOutput,
            ConsolePosition size
        );

        [DllImport(User32DllName)]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport(User32DllName)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Kernel32DllName,
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern bool AllocConsole();

        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get
        /// extended error information, call GetLastError.
        /// </returns>
        [DllImport(Kernel32DllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
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

        [DllImport(Kernel32DllName)]
        public static extern bool SetStdHandle(StdHandle nStdHandle, IntPtr hHandle);

        [DllImport(Kernel32DllName, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFile(string lpFileName
            , [MarshalAs(UnmanagedType.U4)] DesiredAccess dwDesiredAccess
            , [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode
            , IntPtr lpSecurityAttributes
            , [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition
            , [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes
            , IntPtr hTemplateFile
        );

        [DllImport(Kernel32DllName,
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport(Kernel32DllName, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile
        );
    }
}