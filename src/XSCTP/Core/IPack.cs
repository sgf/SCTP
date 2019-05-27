using System;
using System.Collections.Generic;
using System.Text;
using WinDivertSharp;
using wd = WinDivertSharp.WinDivert;

namespace XSCTP
{
    interface IPack
    {
        void Filter()
        {
            var ip = wd.WinDivertOpen("", WinDivertLayer.Forward, 0, WinDivertOpenFlags.Debug);
            uint readLen = 0;
            var wdb = new WinDivertBuffer(4096);
            var wda = new WinDivertAddress();
            wd.WinDivertRecv(ip, wdb, ref wda, ref readLen);
            var sendBuff= new WinDivertBuffer(4096);
            wd.WinDivertSendEx(ip, sendBuff, sendBuff.Length, 0, ref wda);
            //
        }

        bool Send(string msg) {
            return false;
        }

        bool Receive(string msg)
        {
            return false;
        }


        //Memory<byte> Get() {
        //    Memory<byte>.Empty.
        //}

    }


}
