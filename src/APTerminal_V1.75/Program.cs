using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using System.Threading;

#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif

namespace APTerminal
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
#if WindowsCE
            if (IsInstanceRunning())
                return;
#endif
            Tools.timer = Environment.TickCount;
            Application.Run(new MainForm());
        }

        #region OpenNETCF native interface to mutex generation (version 1.4 of the SDF)

        public const Int32 NATIVE_ERROR_ALREADY_EXISTS = 183;

        #region P/Invoke Commands for Mutexes
        [DllImport("coredll.dll", EntryPoint = "CreateMutex", SetLastError = true)]
        public static extern IntPtr CreateMutex(
            IntPtr lpMutexAttributes,
            bool InitialOwner,
            string MutexName);

        [DllImport("coredll.dll", EntryPoint = "ReleaseMutex", SetLastError = true)]
        public static extern bool ReleaseMutex(IntPtr hMutex);
        #endregion

        public static bool IsInstanceRunning()
        {
            IntPtr hMutex = CreateMutex(IntPtr.Zero, true, "ApplicationName");
            if (hMutex == IntPtr.Zero)
                throw new ApplicationException("Failure creating mutex: "
                    + Marshal.GetLastWin32Error().ToString("X"));

            if (Marshal.GetLastWin32Error() == NATIVE_ERROR_ALREADY_EXISTS)
                return true;
            else
                return false;
        }
        #endregion
    }
}