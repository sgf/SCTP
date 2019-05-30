using SCTP.Pack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SctpTestConsoleApp
{
    public class SCTP
    {



        private NetDevice Device;

        public void Run()
        {
            Device.Run();
        }


        public unsafe void Connect()
        {
            CommonHeader head = new CommonHeader();
            head.DstPort = 10000;
            head.SrcPort = 10001;
            head.VerTag = 1000;
            head.Chksum = 100;
            var buff = MemoryPool<byte>.Shared.Rent(sizeof(CommonHeader));
            head.ToBig(buff.Memory.Span);
            Device.Send(buff.Memory.Span);
        }



        ///// <summary>
        ///// 连接到远程服务器
        ///// </summary>
        ///// <param name="ipport">Remote EndPoint</param>
        ///// <param name="TimeOut"> timeout time(Second)</param>
        ///// <returns></returns>
        //public ValueTask<(bool ok, string msg)> ConnectAsync(string ipport, ushort timeout = 2)
        //{
        //    //var (ok, ipp) = IPPort.Prase(ipport);
        //    //if (!ok) return new ValueTask<(bool ok, string msg)>((false, "ipport 格式不正确"));
        //    //var (okBind, msgBind) = Bind(ipp);
        //    //if (!okBind) return new ValueTask<(bool ok, string msg)>((false, msgBind));
        //    //RemoteEnd = ipp;

        //    //SCTP 发包收包
        //}




    }
}
