using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XSCTP
{
    class SCTP
    {

        /// <summary>
        /// 远端
        /// </summary>
        public IPPort RemoteEnd { get; private set; }


        /// <summary>
        /// 连接到远程服务器
        /// </summary>
        /// <param name="ipport">Remote EndPoint</param>
        /// <param name="TimeOut"> timeout time(Second)</param>
        /// <returns></returns>
        public ValueTask<(bool ok, string msg)> ConnectAsync(string ipport, ushort timeout = 2)
        {
            var (ok, ipp) = IPPort.Prase(ipport);
            if (!ok) return new ValueTask<(bool ok, string msg)>((false, "ipport 格式不正确"));
            var (okBind, msgBind) = Bind(ipp);
            if (!okBind) return new ValueTask<(bool ok, string msg)>((false, msgBind));
            RemoteEnd = ipp;
            //SCTP 发包收包
        }

        /// <summary>
        /// 绑定端口
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static (bool ok, string msg) Bind(IPPort end)
        {
            //获取进程ID int pid = Process.GetCurrentProcess().Id;
            var key = $"SCTP{end}"; //SCTP+IP[固定长度]+端口  //+进程ID 先排除
            using var mx = new Mutex(true, key);
            if (!mx.WaitOne(100, true)) return (false, "端口已被占用");
            return (true, "");
        }


        public class IPPort
        {
            public IPPort(IAddress ip, ushort port)
            {
                IP = ip;
                Port = port;
            }

            public IAddress IP;
            public ushort Port;

            public override string ToString()
            {
                return $"{IP}{Port}";
            }
            public static (bool, IPPort) Prase(string ipport)
            {
                IPEndPoint.TryParse(ipport);

                var ip_post = ipport.Split(":");
                if (ip_post.Length != 2) return (false, default!);
                var addr = ip_post[0];
                var port = ip_post[1];



                    return (true, new IPPort());
            }
        }

    }
