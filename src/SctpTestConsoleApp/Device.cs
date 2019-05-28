using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDivertSharp;
using WinDivertSharp.WinAPI;
using wd = WinDivertSharp.WinDivert;

namespace SctpTestConsoleApp
{
    public class Device : IDisposable
    {
        /// <summary>
        /// WinDivertPointer
        /// </summary>
        private IntPtr WDHandle;

        public Device()
        {
            WDHandle = wd.WinDivertOpen(@"outbound and inbound
and ip.Protocol=132 "//SCTP协议
 , WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueLen, WinDivertParam_QueueLenMax);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueTime, WinDivertParam_QueueTimeMax);
            wd.WinDivertSetParam(WDHandle, WinDivertParam.QueueSize, WinDivertParam_QueueSizeMax);
            Task.Run(Run);
        }

        const int
            WinDivertParam_QueueLenMax = 16384,// 16kb
             WinDivertParam_QueueTimeMax = 16000,   // 16s
        WinDivertParam_QueueSizeMax = 33554432;   // 32MB
        const int ERROR_IO_PENDING = 997;



        public async void Run()
        {
            Pipe pipe = new Pipe();
            var t1 = Receive(pipe.Writer);
            var t2 = OnMsg(pipe.Reader);
            await Task.WhenAll(t1.AsTask(), t2.AsTask());
        }

        /// <summary>
        /// 以后可以 变成 三种状态 1:started 2:running 3:stoping 4:stoped
        /// </summary>
        public bool Running = false;


        private async ValueTask Receive(PipeWriter writer)
        {
            var workspace = writer.GetMemory(8192);
            var pack = new WinDivertBuffer(workspace.ToArray());
            WinDivertAddress addr = default;
            NativeOverlapped nol = default;
            IntPtr recvEvent = Kernel32.CreateEvent(IntPtr.Zero, false, false, IntPtr.Zero);
            nol.EventHandle = recvEvent;
            WinDivertParseResult result = new WinDivertParseResult();
            uint recvAsyncIoLen = 0;
            //writer.WriteAsync()
            try
            {
                while (Running)
                {
                    uint readLen = 0;
                    recvAsyncIoLen = 0;
                    addr.Reset();
                    if (!wd.WinDivertRecvEx(WDHandle, pack, 0, ref addr, ref recvAsyncIoLen, ref nol))
                    {
                        var error = Marshal.GetLastWin32Error();
                        if (error != ERROR_IO_PENDING)
                        {
                            Debug.WriteLine($"Unknown IO error ID {error} while awaiting overlapped result.");
                            continue;
                        }

                        switch (Kernel32.WaitForSingleObject(recvEvent, 500))
                        {
                            case (uint)WaitForSingleObjectResult.WaitObject0:
                                break;
                            case (uint)WaitForSingleObjectResult.WaitTimeout:
                                Debug.WriteLine($"Failed to read packet from WinDivert by timeout with Win32 error {Marshal.GetLastWin32Error()}.");
                                goto end;
                            default:
                                Debug.WriteLine($"Failed to read packet from WinDivert with Win32 error  {Marshal.GetLastWin32Error()}.");
                                goto end;
                        }

                        //while (Kernel32.WaitForSingleObject(recvEvent, 500) == (uint)WaitForSingleObjectResult.WaitTimeout)
                        //{
                        //    if (!Running)
                        //        goto end;
                        //}

                        if (!Kernel32.GetOverlappedResult(WDHandle, ref nol, ref recvAsyncIoLen, false))
                        {
                            Debug.WriteLine($"Failed to get overlapped result.");
                            Kernel32.CloseHandle(recvEvent);
                            continue;
                        }
                        readLen = recvAsyncIoLen;
                    }

                    Kernel32.CloseHandle(recvEvent);
                    Debug.WriteLine("Read packet {0}", readLen);
                    result = WinDivert.WinDivertHelperParsePacket(pack, readLen);

                    writer.Advance((int)readLen);
                    await writer.FlushAsync();
                    if (addr.Direction == WinDivertDirection.Inbound)
                        Debug.WriteLine("inbound");
                    DisplayPackInfo(result);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error in read thread: {0}", ex);
            }
            finally
            {
                wd.WinDivertClose(WDHandle);
                Kernel32.CloseHandle(recvEvent);
            }
            end:;


            unsafe void DisplayPackInfo(WinDivertParseResult pack)
            {

                if (result.IPv4Header != null && result.TcpHeader != null)
                    Debug.WriteLine($"V4 TCP packet {addr.Direction} from {result.IPv4Header->SrcAddr}:{result.TcpHeader->SrcPort} to {result.IPv4Header->DstAddr}:{result.TcpHeader->DstPort}");

                else if (result.IPv6Header != null && result.TcpHeader != null)
                    Debug.WriteLine($"V4 TCP packet {addr.Direction} from {result.IPv6Header->SrcAddr}:{result.TcpHeader->SrcPort} to {result.IPv6Header->DstAddr}:{result.TcpHeader->DstPort}");

            }

        }

        public unsafe async ValueTask OnMsg(PipeReader reader)
        {

        }


        public unsafe ValueTask Send(Span<byte> data)
        {
            if (Running)
            {
                var sendBuff = new WinDivertBuffer(data.ToArray());
                NativeOverlapped nol = default;
                WinDivertAddress addr = default;
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



        public void Dispose()
        {
            wd.WinDivertClose(WDHandle);
        }
    }
}
