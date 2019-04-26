using System;
using System.Collections.Generic;

namespace SCTP
{
    /// <summary>
    /// 可以是IPV4 也可以是IPV6
    /// </summary>
    public class Addr
    {
        public Addr(string ip)
        {


        }

        public byte Ver { get; set; }
        public bool IsV4 { get; set; }
        public bool IsV6 { get; set; }

    }

    /// <summary>
    /// 端点(端点可以有多个地址，但端点之间能且仅能建立一条连接 即：连接可以跨地址和端口)
    /// </summary>
    public class EndPoint
    {
        public EndPoint(ushort port, string[] ips)
        {
            foreach (var ip in ips) { Address.Add(new Addr(ip)); }
            Port = port;
        }

        /// <summary>
        /// 多个地址
        /// </summary>
        public List<Addr> Address = new List<Addr>();
        public ushort Port;

    }
    /// <summary>
    /// 流（单向的数据通道）
    /// </summary>
    public class Stream
    {


    }


}
