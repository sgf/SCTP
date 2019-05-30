using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDivertSharp;
using WinDivertSharp.WinAPI;
using wd = WinDivertSharp.WinDivert;
using XDivert;
using System.Net;

namespace SctpTestConsoleApp
{
    public class NetDevice : IDisposable
    {
        /// <summary>
        /// WinDivertPointer
        /// </summary>
        private IntPtr WDHandle;

        public NetDevice()
        {
            WDHandle = wd.WinDivertOpen(
            /*outbound 暂时不需要 outbound and */ @"inbound
and ip.Protocol=132 "//SCTP协议
, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueLen, WinDivertParam_QueueLenMax);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueTime, WinDivertParam_QueueTimeMax);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueSize, WinDivertParam_QueueSizeMax);
            Task.Run(Run);
        }

        ///只有 WINDIVERT_LAYER_NETWORK WINDIVERT_LAYER_NETWORK_FORWARD 支持注入


        const int
            WinDivertParam_QueueLenMax = 16384,// 16kb
             WinDivertParam_QueueTimeMax = 16000,   // 16s
        WinDivertParam_QueueSizeMax = 33554432;   // 32MB
        const int ERROR_IO_PENDING = 997;



        public async void Run()
        {
            inputPipe = new Pipe();
            var t1 = FilterPackets(inputPipe.Writer);
            var t2 = OnMsg(inputPipe.Reader);
            await Task.WhenAll(t1.AsTask(), t2.AsTask());
        }

        /// <summary>
        /// 以后可以 变成 三种状态 1:started 2:running 3:stoping 4:stoped
        /// </summary>
        public bool Running = false;


        private Pipe inputPipe = new Pipe();

        private async ValueTask FilterPackets(PipeWriter writer)
        {
            var workspace = writer.GetMemory(8192);
            var pack = new WinDivertBuffer(workspace.ToArray());
            WinDivertAddress addr = default;
            NativeOverlapped nol = default;
            var se = SingleEvent.Create();
            nol.EventHandle = se.Event;
            WinDivertParseResult result = new WinDivertParseResult();
            uint readLen = 0;

            //writer.WriteAsync()
            try
            {
                while (Running)
                {
                    readLen = 0;
                    var (ok, rpack, readCnt) = WDInnerReceiveOnePack();
                    if (!ok) goto end;
                    writer.Advance((int)readLen);
                    await writer.FlushAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error in read thread: {0}", ex);
            }
            finally
            {
                wd.WinDivertClose(WDHandle);
                se.Close();
            }
            end:;

            (bool ok, WinDivertParseResult pack, uint readLen) WDInnerReceiveOnePack()
            {
                try
                {
                    //var sp = writer.GetSpan();
                    addr.Reset();
                    again:
                    if (!Running) goto end_sub;
                    if (!wd.WinDivertRecvEx(WDHandle, pack, 0, ref addr, ref readLen, ref nol))
                    {
                        if (!Win32.LasErr(ERROR_IO_PENDING, "Unknown IO error ID while awaiting overlapped result."))
                            goto again;
                        if (!se.Wait(existsAction: () => Running))
                            goto end_sub;

                        if (!Kernel32.GetOverlappedResult(WDHandle, ref nol, ref readLen, false))
                        {
                            Debug.WriteLine($"Failed to get overlapped result.");
                            se.Close();
                            goto again;
                        }
                    }
                    se.Close();//这个可能位置不太对？关掉是否影响下一轮的接收
                    Debug.WriteLine("Read packet {0}", readLen);
                    result = WinDivert.WinDivertHelperParsePacket(pack, readLen);
                    if (addr.Direction == WinDivertDirection.Inbound)
                        Debug.WriteLine("inbound");
                    DisplayPackInfo(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fatal error in read thread: {0}", ex);
                    goto end_sub;
                }
                finally
                {
                    wd.WinDivertClose(WDHandle);
                    se.Close();
                }

                return (true, result, readLen);

                end_sub:
                return (false, result, readLen);

            }
            unsafe void DisplayPackInfo(WinDivertParseResult pack)
            {
                if (result.IPv4Header != null && result.TcpHeader != null)
                    Debug.WriteLine($"V4 TCP packet {addr.Direction} from {result.IPv4Header->SrcAddr}:{result.TcpHeader->SrcPort} to {result.IPv4Header->DstAddr}:{result.TcpHeader->DstPort}");

                else if (result.IPv6Header != null && result.TcpHeader != null)
                    Debug.WriteLine($"V4 TCP packet {addr.Direction} from {result.IPv6Header->SrcAddr}:{result.TcpHeader->SrcPort} to {result.IPv6Header->DstAddr}:{result.TcpHeader->DstPort}");

            }

        }

        public async ValueTask OnMsg(PipeReader reader)
        {
            while (Running)
            {

                var readresult = await reader.ReadAsync();
                var availableBuff = readresult.Buffer;
                //availableBuff.PositionOf<byte>()
                //parser.Prase()

            }



            //reader.AdvanceTo()


        }


        public unsafe void Send(Span<byte> data)
        {
            if (Running)
            {
                IPv4Header iphdr = new IPv4Header();
                iphdr.SrcAddr = IPAddress.Parse("127.0.0.1");
                iphdr.DstAddr = IPAddress.Parse("127.0.0.1");
                iphdr.HdrLength = (byte)sizeof(IPv4Header);
                iphdr.Version = 4;
                iphdr.Protocol = 132;//SCTP协议

                .iphdr
                data.Slice(8).Fill()

                var sendBuff = new WinDivertBuffer(data.ToArray());
                NativeOverlapped nol = default;
                WinDivertAddress addr = new WinDivertAddress();
                addr.Direction = WinDivertDirection.Outbound;
                addr.Loopback = true;
                uint sendLen = 0;
                //wd.WinDivertSendEx(WDHandle, sendBuff, sendBuff.Length, 0, ref addr, ref sendLen, ref nol);
                if (!WinDivert.WinDivertSendEx(WDHandle, sendBuff, sendBuff.Length, 0, ref addr))
                {
                    Debug.WriteLine("Write Err: {0}", Marshal.GetLastWin32Error());
                }
            }
        }

        public void RecvEx()
        {

        }

        private bool Closeing = false;
        public void Dispose()
        {
            if (!Closeing && WDHandle != IntPtr.Zero)
            {
                Closeing = true;
                wd.WinDivertClose(WDHandle);
            }
        }
    }




}
