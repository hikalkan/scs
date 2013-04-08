using System;
using System.Runtime.InteropServices;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This class is used to flash window caption / taskbar button when a message received
    /// and window is not active.
    /// </summary>
    public static class WindowsHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public FlashWindowFlags dwFlags; //uint
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        public enum FlashWindowFlags : uint
        {
            /// <summary>
            /// Stop flashing. The system restores the window to its original state.
            /// </summary>
            FLASHW_STOP = 0,

            /// <summary>
            /// Flash the window caption.
            /// </summary>
            FLASHW_CAPTION = 1,

            /// <summary>
            /// Flash the taskbar button.
            /// </summary>
            FLASHW_TRAY = 2,

            /// <summary>
            /// Flash both the window caption and taskbar button.
            /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
            /// </summary>
            FLASHW_ALL = 3,

            /// <summary>
            /// Flash continuously, until the FLASHW_STOP flag is set.
            /// </summary>
            FLASHW_TIMER = 4,

            /// <summary>
            /// Flash continuously until the window comes to the foreground.
            /// </summary>
            FLASHW_TIMERNOFG = 12
        }


        public static bool FlashWindow(IntPtr hWnd,
                                       FlashWindowFlags fOptions,
                                       uint FlashCount,
                                       uint FlashRate)
        {
            if (IntPtr.Zero != hWnd)
            {
                FLASHWINFO fi = new FLASHWINFO();
                fi.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
                fi.dwFlags = fOptions;
                fi.uCount = FlashCount;
                fi.dwTimeout = FlashRate;
                fi.hwnd = hWnd;

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        public static bool StopFlashingWindow(IntPtr hWnd)
        {
            if (IntPtr.Zero != hWnd)
            {
                FLASHWINFO fi = new FLASHWINFO();
                fi.cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO));
                fi.dwFlags = (uint)FlashWindowFlags.FLASHW_STOP;
                fi.hwnd = hWnd;

                return FlashWindowEx(ref fi);
            }
            return false;
        }
    }
}
