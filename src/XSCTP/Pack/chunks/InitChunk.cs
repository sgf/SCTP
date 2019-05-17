using System;
using System.Collections.Generic;
using System.Text;

namespace XSCTP.Pack.chunks
{
    /*
    +---------------+---------------+-------------------------------+
    |                         Initiate Tag                          |
    +---------------------------------------------------------------+
    |           Advertised Receiver Window Credit(a_rwnd)          |
    +-------------------------------+-------------------------------+
    |  Number of Outbound Streams   |  Number of Inbound Streams    |
    +-------------------------------+-------------------------------+
    |                          Initial TSN                          |
    +---------------------------------------------------------------+
    |                                                               |
    |              Optional/Variable-Length Parameters              |
    |                                                               |
    +---------------------------------------------------------------+*/

    /// <summary>
    /// 3.3.2. Initiation (INIT) (1) --RFC4960
    /// </summary>
    class InitChunk : IChunk
    {  // byte Type where value == 1;
       /// <summary>
       /// 启动标签。INIT的接收方(响应端)记录启动标签参数的值。这个值必须被放置到INIT的接收方发送的与该偶联相关的每个SCTP分组中的验证标签字段中。启动标签允许除0以外的的任何值。如果在收到的INIT数据块中的启动标签为0，则接收方必须作为错误处理，并且发送ABORT数据块中止该偶联。
       /// </summary>
        uint InitiateTag;

        uint AdvertisedReceiverWindowCredit;
        ushort NumberofOutboundStreams;
        ushort NumberofInboundStreams;
        uint InitialTSN;
        public InitOptionalOrVariableParameter[] OptionalOrVariableParameters; //optional [|ChunkLength > 20|] array<InitOptionalOrVariableParameter> OptionalOrVariableParameters;

        //IPv4 Address(Note 1) Optional 5
        //IPv6 Address(Note 1) Optional 6 
        //Cookie Preservative Optional 9
        //Reserved for ECN Capable(Note 2) Optional 32768 (0x8000) 
        //Host Name Address(Note 3) Optional 11
        //Supported Address Types(Note 4) Optional 12

    }
}
