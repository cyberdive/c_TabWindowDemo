using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TabWindow
{
    public class Win32Helper
    {
        internal const int WM_ENTERSIZEMOVE = 0x0231;
        internal const int WM_EXITSIZEMOVE = 0x0232;
        internal const int WM_MOVE = 0x0003;
        internal const int GW_HWNDNEXT = 2;
        internal const int GW_HWNDPREV = 3;

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("User32", SetLastError = true)]
        internal static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
    }
}
