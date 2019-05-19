using System;
using System.Collections.Generic;
using System.Text;
using pk= PacketDotNet;

namespace XSCTP
{

    struct IP4_Hdr
    {
        public IPv4 InAddr;
        public IPv4 DstAddr;
    }

    struct IP6_Hdr
    {
        public IPv6 InAddr;
        public IPv6 DstAddr;
    }


}
