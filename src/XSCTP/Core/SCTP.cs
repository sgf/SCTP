using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XSCTP
{
    class SCTP
    {
        /// <summary>
        /// 连接到远程服务器
        /// </summary>
        /// <param name="ipport"></param>
        /// <param name="TimeOut"> timeout time(Second)</param>
        /// <returns></returns>
        public static ValueTask<bool> ConnectAsync(string ipport, ushort timeout = 2)
        {
            return new ValueTask<bool>(true);
        }

        public void M4Mutex(string ip, ushort port)
        {
            //int pid = Process.GetCurrentProcess().Id;


            IAddress address;
            //SCTP+IP[固定长度]+端口  //+进程ID 先排除
            using var mx = new Mutex(true, $"SCTP{ip}{port}");
            Console.WriteLine("正在等待 the mutex");
            //申请
            if (!mx.WaitOne(50, true)) Console.WriteLine("端口已被占用");

        }


    }





}
