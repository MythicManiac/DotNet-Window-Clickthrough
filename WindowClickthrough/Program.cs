using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace WindowClickthrough
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        public static Random random = new Random();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Process[] processList = Process.GetProcesses();
            Windows = new List<WindowThingy>();
            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if(wnd != IntPtr.Zero)
                    Windows.Add(new WindowThingy(wnd, random.Next(256)));
                return true;
            }, IntPtr.Zero);
            Console.WriteLine(Windows.Count);
            Run();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
        }

        private class WindowThingy
        {
            public IntPtr Handle;
            public int Current;
            public bool Increment;

            public WindowThingy(IntPtr handle, int current) { Handle = handle; Current = current; }
        }

        private static List<WindowThingy> Windows;
        private static byte delta = 5;

        public static void Run()
        {
            for (int i = 0; i < Windows.Count; i++)
            {
                SetWindowLong(Windows[i].Handle, GWL_EXSTYLE, GetWindowLong(Windows[i].Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            }

            while (true)
            {
                for (int i = 0; i < Windows.Count; i++)
                {
                    if (Windows[i].Increment)
                        Windows[i].Current += delta;
                    else
                        Windows[i].Current -= delta;

                    if (Windows[i].Current < 0)
                    {
                        Windows[i].Increment = true;
                        Windows[i].Current = -Windows[i].Current;
                    }
                    if (Windows[i].Current > 255)
                    {
                        Windows[i].Increment = false;
                        Windows[i].Current = 255 - (Windows[i].Current - 255);
                    }

                    SetLayeredWindowAttributes(Windows[i].Handle, 0, (byte)Windows[i].Current, 0x2);
                }
            }
            Thread.Sleep(16);
        }
    }
}
