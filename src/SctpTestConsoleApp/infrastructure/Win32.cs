using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SctpTestConsoleApp
{

    public class Win32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LasErr(int error, string msg)
        {
            var _error = Marshal.GetLastWin32Error();
            if (_error == error) return true;
            Debug.WriteLine($"Error:{_error} Msg:{msg}");
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LasErr(string msg)
        {
            var _error = Marshal.GetLastWin32Error();
            Debug.WriteLine($"Error:{_error} Msg:{msg}");
        }
    }
}
