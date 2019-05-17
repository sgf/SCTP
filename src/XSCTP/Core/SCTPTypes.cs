using System;
using System.Collections.Generic;
using System.Text;
using pk= PacketDotNet;

namespace XSCTP
{

    struct IP4_Hdr
    {
        public IP4Address InAddr;
        public IP4Address DstAddr;
    }

    struct IP6_Hdr
    {
        public IP6Address InAddr;
        public IP6Address DstAddr;
    }


}
