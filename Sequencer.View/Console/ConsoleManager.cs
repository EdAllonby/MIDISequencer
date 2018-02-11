using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Sequencer.View.Console
{
    public static class ConsoleWindow
    {
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        public static void Hide()
        {
            UnsafeNativeMethods.FreeConsole();
        }

        public static void Show(int bufferWidth = -1, bool breakRedirection = true, int consoleWidth = 800, int consoleHeight = 1000, int bufferHeight = 9999)
        {
            UnsafeNativeMethods.AllocConsole();
            IntPtr stdOut = InvalidHandleValue;

            if (breakRedirection)
            {
                UnredirectConsole(out stdOut, out _, out _);
            }

            Stream outStream = System.Console.OpenStandardOutput();
            Stream errStream = System.Console.OpenStandardError();

            Encoding encoding = Encoding.GetEncoding(UnsafeNativeMethods.MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(outStream, encoding), standardError = new StreamWriter(errStream, encoding);

            if (bufferWidth == -1)
            {
                bufferWidth = 180;
            }

            try
            {
                standardOutput.AutoFlush = true;
                standardError.AutoFlush = true;
                System.Console.SetOut(standardOutput);
                System.Console.SetError(standardError);
                if (breakRedirection)
                {
                    var coord = new ConsolePosition
                    {
                        X = (short) bufferWidth,
                        Y = (short) bufferHeight
                    };

                    UnsafeNativeMethods.SetConsoleScreenBufferSize(stdOut, coord);
                }
                else
                {
                    System.Console.SetBufferSize(bufferWidth, bufferHeight);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            try
            {
                IntPtr hConsole = UnsafeNativeMethods.GetConsoleWindow();
                UnsafeNativeMethods.MoveWindow(hConsole, 0, 500, 1000, 800, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            UnsafeNativeMethods.ShowWindow(p.MainWindowHandle, 3);
        }

        private static void UnredirectConsole(out IntPtr stdOut, out IntPtr stdIn, out IntPtr stdErr)
        {
            UnsafeNativeMethods.SetStdHandle(StdHandle.Output, stdOut = GetConsoleStandardOutput());
            UnsafeNativeMethods.SetStdHandle(StdHandle.Input, stdIn = GetConsoleStandardInput());
            UnsafeNativeMethods.SetStdHandle(StdHandle.Error, stdErr = GetConsoleStandardError());
        }

        private static IntPtr GetConsoleStandardInput()
        {
            IntPtr handle = UnsafeNativeMethods.CreateFile
            ("CONIN$"
                , DesiredAccess.GenericRead | DesiredAccess.GenericWrite
                , FileShare.ReadWrite
                , IntPtr.Zero
                , FileMode.Open
                , FileAttributes.Normal
                , IntPtr.Zero
            );

            return handle == InvalidHandleValue ? InvalidHandleValue : handle;
        }

        private static IntPtr GetConsoleStandardOutput()
        {
            IntPtr handle = UnsafeNativeMethods.CreateFile
            ("CONOUT$"
                , DesiredAccess.GenericWrite | DesiredAccess.GenericWrite
                , FileShare.ReadWrite
                , IntPtr.Zero
                , FileMode.Open
                , FileAttributes.Normal
                , IntPtr.Zero
            );

            return handle == InvalidHandleValue ? InvalidHandleValue : handle;
        }

        private static IntPtr GetConsoleStandardError()
        {
            IntPtr handle = UnsafeNativeMethods.CreateFile
            ("CONERR$"
                , DesiredAccess.GenericWrite | DesiredAccess.GenericWrite
                , FileShare.ReadWrite
                , IntPtr.Zero
                , FileMode.Open
                , FileAttributes.Normal
                , IntPtr.Zero
            );

            return handle == InvalidHandleValue ? InvalidHandleValue : handle;
        }
    }
}