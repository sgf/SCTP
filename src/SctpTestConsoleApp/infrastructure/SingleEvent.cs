using System;
using System.Collections.Generic;
using System.Text;
using WinDivertSharp.WinAPI;

namespace SctpTestConsoleApp
{

    public struct SingleEvent
    {
        /// <summary>
        /// 等待结果
        /// </summary>
        /// <param name="timeOut">MilliSeconds</param>
        /// <param name="loopWhenTimeOut">超时是否永远重试</param>
        /// <param name="existsAction">超时重试时如果满足条件 是否退出重试loop</param>
        public bool Wait(uint timeOut = 250, bool loopWhenTimeOut = true, Func<bool> existsAction = null)
        {
            again://timeOut Again Loop 
            switch (Kernel32.WaitForSingleObject(Event, timeOut))
            {
                case (uint)WaitForSingleObjectResult.WaitObject0:
                    return true;
                case (uint)WaitForSingleObjectResult.WaitTimeout:
                    Win32.LasErr("Failed to read packet from WinDivert by timeout with Win32 error ");
                    if (loopWhenTimeOut && (existsAction == null | !existsAction())) goto again;
                    break;
                default:
                    Win32.LasErr("Failed to read packet from WinDivert with Win32 error ");
                    break;
            }
            return false;
        }

        public static SingleEvent Create()
        {
            SingleEvent se;
            se.Event = Kernel32.CreateEvent(IntPtr.Zero, false, false, IntPtr.Zero);
            se.Closeing = false;
            return se;
        }

        public void Close()
        {
            if (!Closeing && Event != IntPtr.Zero)
            {
                Closeing = true;
                Kernel32.CloseHandle(Event);
            }
        }
        private bool Closeing;
        public IntPtr Event;

    }


}
