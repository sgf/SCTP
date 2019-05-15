using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{

    //MAC - Message Authentication Code[RFC2104]
    //RTO - Retransmission Timeout
    //RTT - Round-Trip Time                             RTT-往返时间（单个轮回时间）
    //RTTVAR - Round-Trip Time Variation                RTTVAR-往返时间变化
    //SCTP - Stream Control Transmission Protocol       流控传输协议
    //SRTT - Smoothed RTT                               SRTT 平滑的RTT
    //TCB - Transmission Control Block                  传输控制块
    //TLV - Type-Length-Value coding format             类型-长度-值 这类编码格式
    //TSN - Transmission Sequence Number                传输序列号
    //ULP - Upper-Layer Protocol                        上层协议



    interface IChunkValue {
    //    pattern ChunkValue = PayloadData | // 0
    //Initiation | // 1
    //InitiationAcknowledgement | // 2
    //SelectiveAcknowledgement | // 3
    //HeartbeatRequest | // 4
    //HeartbeatAcknowledgement | // 5
    //AbortAssociation | // 6
    //ShutdownAssociation | // 7
    //ShutdownAcknowledgement | // 8
    //OperationError | // 9
    //CookieEcho | // 10
    //CookieAcknowledgement | // 11
    //EcnEcho | // 12
    //Cwr | // 13
    //ShutdownComplete;              // 14
    }




    /********************************************** Chunk Types **********************************************/
    class PayloadData // 0
    {
        public byte Type { get; set; }//byte Type where value == 0;

        public byte Reserved { get; set; }//byte Reserved with BinaryEncoding { Width = 5};

        public byte U { get; set; } //byte U with BinaryEncoding { Width = 1};

        public byte B { get; set; } //  byte B with BinaryEncoding { Width = 1};

        public byte E { get; set; } //   byte E with BinaryEncoding { Width = 1};

        ushort Length;
        uint TSN;
        ushort StreamIdentifier;
        ushort StreamSequenceNumber;
        uint PayloadProtocolIdentifier;

        byte[] UserData { get; set; }// binary UserData with BinaryEncoding { Length = Length - 16};

    }

    class Initiation // 1
    {

        byte Type;  // byte Type where value == 1;
        byte ChunkFlags;
        ushort ChunkLength;
        uint InitiateTag;
        uint AdvertisedReceiverWindowCredit;
        ushort NumberofOutboundStreams;
        ushort NumberofInboundStreams;
        uint InitialTSN;
        optional[| ChunkLength > 20 |]
        //array<InitOptionalOrVariableParameter> OptionalOrVariableParameters;

        public InitOptionalOrVariableParameter[] OptionalOrVariableParameters;
    }


    //Optional/Variable-Length Parameters in INIT
    interface InitOptionalOrVariableParameter
    {

    }


    struct AddressHead
    {
        public static AddressHead New_IPv4AddressParameter = new AddressHead { Type = AddressType.V4, Length = 8 },
            New_IPv6AddressParameter = new AddressHead { Type = AddressType.V6, Length = 20 };
        public AddressType Type;
        public ushort Length;

        public static AddressHead New_HostNameAddress(ushort len)
        {
            return new AddressHead { Type = AddressType.HostName, Length = len };
        }
    }

    public enum AddressType : ushort
    {
        V4 = 5,
        V6 = 6,
        HostName = 11
    }

    struct IPv4AddressParameter : IUnresolvableAddress, InitOptionalOrVariableParameter, InitAckOptionalOrVariableParameter, NewAddressTlv
    {
        private static AddressHead _head = new AddressHead { Type = AddressType.V4, Length = 8 };

        public IPv4AddressParameter(IPv4Address ipv4)
        {
            Head = _head;
            IPv4Address = ipv4;
        }

        public AddressHead Head { get; }
        public IPv4Address IPv4Address;

        public static IPv4AddressParameter New(IPv4Address ipv4)
        {
            return new IPv4AddressParameter(ipv4);
        }
    }

    struct IPv6AddressParameter : IUnresolvableAddress, InitOptionalOrVariableParameter, InitAckOptionalOrVariableParameter, NewAddressTlv
    {
        private static AddressHead _head = new AddressHead { Type = AddressType.V6, Length = 20 };

        public IPv6AddressParameter(IPv6Address ipv6)
        {
            Head = _head;
            IPv6Address = ipv6;
        }

        public AddressHead Head { get; }
        public IPv6Address IPv6Address;

        public static IPv6AddressParameter New(IPv6Address ipv6)
        {
            return new IPv6AddressParameter(ipv6);
        }
    }

    struct HostNameAddressParameter : IUnresolvableAddress, InitOptionalOrVariableParameter, InitAckOptionalOrVariableParameter, NewAddressTlv
    {
        public HostNameAddressParameter(string hostName)
        {
            HostName = hostName;
            HostNameData = ASCIIEncoding.ASCII.GetBytes(hostName);
            Head = new AddressHead() { Type = AddressType.HostName, Length = (ushort)HostNameData.Length };
        }
        public AddressHead Head { get; }
        public string HostName;// HostName with BinaryEncoding { Length = Length - 4,TextEncoding = TextEncoding.ASCII };
        public byte[] HostNameData;
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    struct CookiePreservative : InitOptionalOrVariableParameter
    {
        public AddressHead Head;//Type=9 Length=8
        public uint SuggestedCookieLifeSpanIncrement;
    }


    class SupportedAddressTypes : InitOptionalOrVariableParameter
    {
        public AddressHead Head { get; }//Type=12 Length=?
        ushort[] AddressTypes;// array<ushort> AddressTypes with BinaryEncoding { Length = ((Length - 4) / 2)};
    }


    class InitiationAcknowledgement // 2
    {
        byte Type = 2;
        byte ChunkFlags;
        ushort ChunkLength;
        uint InitiateTag;
        uint AdvertisedReceiverWindowCredit;
        ushort NumberOfOutboundStreams;
        ushort NumberOfInboundStreams;
        uint InitialTSN;
        array<InitAckOptionalOrVariableParameter> OptionalOrVariableParameters;
    }



    interface InitAckOptionalOrVariableParameter
    {

    }

    class StateCookie : InitAckOptionalOrVariableParameter
    {
        ushort Type = 7;
        ushort Length;
        binary Value with BinaryEncoding { Length = Length - 4};
    }



    class EcnParameter : InitOptionalOrVariableParameter, InitAckOptionalOrVariableParameter
    {
        ushort Type = 32768;
        ushort Length = 4;
    }

    class SelectiveAcknowledgement // 3
    {
        byte Type = 3;
        byte ChunkFlags;
        ushort ChunkLength;
        uint CumulativeTsnAck;
        uint AdvertisedReceiverWindowCredit;
        ushort NumberOfGapAckBlocks;
        ushort NumberOfDuplicateTsns;
        array<GapAckBlock> GapAckBlocks with BinaryEncoding { Length = NumberOfGapAckBlocks };
        array<uint> DuplicateTsns with BinaryEncoding { Length = NumberOfDuplicateTsns };
    }

    class GapAckBlock
    {
        ushort Start;
        ushort End;
    }

    class HeartbeatRequest // 4
    {
        byte Type = 4;
        byte ChunkFlags;
        ushort HeartbeatLength;
        HeartbeatInformation HeartbeatInformationTlv;
    }

    class HeartbeatInformation
    {
        ushort HeartbeatInfoType = 1;
        ushort HBInfoLength;
        binary SenderSpecificHeartbeatInfo with BinaryEncoding { Length = HBInfoLength - 4};
    }

    class HeartbeatAcknowledgement // 5
    {
        byte Type = 5;
        byte ChunkFlags;
        ushort HeartbeatAckLength;
        HeartbeatInformation HeartbeatInformationTlv;
    }

    class AbortAssociation // 6
    {
        byte Type = 6;
        byte Reserved with BinaryEncoding { Width = 7};
        byte T with BinaryEncoding { Width = 1};
        ushort Length;
        optional[| Length > 4 |]
        array<CauseOfError> ErrorCauses;
    }

    class ShutdownAssociation // 7
    {
        byte Type = 7;
        byte ChunkFlags;
        ushort Length = 8;
        uint CumulativeTsnAck;
    }

    class ShutdownAcknowledgement // 8
    {
        byte Type = 8;
        byte ChunkFlags;
        ushort Length = 4;
    }

    class OperationError // 9
    {
        byte Type = 9;
        byte ChunkFlags;
        ushort Length;
        CauseOfError[] ErrorCauses;
        //array<CauseOfError> ErrorCauses;
    }


    interface CauseOfError {

        //pattern CauseOfError = InvalidStreamIdentifier | // 1
        //    MissingMandatoryParameter | // 2
        //    StaleCookieError | // 3
        //    OutOfResource | // 4
        //    UnresolvableAddress | // 5
        //    UnrecognizedChunkType | // 6
        //    InvalidMandatoryParameter | // 7
        //    UnrecognizedParameters | // 8
        //    NoUserData | // 9
        //    CookieReceivedWhileShuttingDown | // 10
        //    RestartOfAnAssociationWithNewAddresses | // 11
        //    UserInitiatedAbort | // 12
        //    ProtocolViolation;                           // 13
    }





    interface IAddress
    {
        AddressHead Head { get; }
        AddressType Type { get { return Head.Type; } }
        ushort Length { get { return Head.Length; } }
        ref AddressHead ReadHead(Memory<byte> buff)
        {
            return ref buff.Read<AddressHead>();
        }
    }

    interface IUnresolvableAddress : IAddress
    {


    }

    class NoUserData
    {
        ushort CauseCode = 9;
        ushort CauseLength = 8;
        uint TsnValue;
    }

    class CookieReceivedWhileShuttingDown
    {
        ushort CauseCode = 10;
        ushort CauseLength = 4;
    }


    class CookieEcho // 10
    {
        byte Type = 10;
        byte ChunkFlags;
        ushort Length;
        byte[] Cookie;//binary Cookie with BinaryEncoding { Length = Length - 4};
    }

    class CookieAcknowledgement // 11
    {
        byte Type = 11;
        byte ChunkFlags;
        ushort Length = 4;
    }

    class ShutdownComplete // 14
    {
        byte Type = 14;
        byte Reserved with BinaryEncoding { Width = 7};
        byte T with BinaryEncoding { Width = 1};
        ushort Length = 4;
    }

    class EcnEcho // 12
    {
        byte Type = 12;
        byte ChunkFlags = 0;
        ushort Length = 8;
        uint LowestTsnNumber;
    }

    class Cwr // 13
    {
        byte Type = 13;
        byte ChunkFlags = 0;
        ushort Length = 8;
        uint LowestTsnNumber;
    }

    /********************************************** functions **********************************************/
    binary MakeBinary(binary sourceAddress, binary destinationAddress, ushort sourcePort, ushort destinationPort)
    {
        return sourceAddress + destinationAddress + sourcePort.ToBinary() + destinationPort.ToBinary();
    }

    // For summary
    string GetChunkName(ChunkValue chunk)
    {
        switch (chunk)
        {
            case payloadData: PayloadData => return "Payload Data";
            case initiation: Initiation => return "Initiation";
            case initiationAcknowledgement: InitiationAcknowledgement => return "Initiation Acknowledgement";
            case selectiveAcknowledgement: SelectiveAcknowledgement => return "Selective Acknowledgement";
            case heartbeatRequest: HeartbeatRequest => return "Heartbeat Request";
            case heartbeatAcknowledgement: HeartbeatAcknowledgement => return "Heartbeat Acknowledgement";
            case abortAssociation: AbortAssociation => return "Abort Association";
            case shutdownAssociation: ShutdownAssociation => return "Shutdown Association";
            case shutdownAcknowledgement: ShutdownAcknowledgement => return "Shutdown Acknowledgement";
            case operationError: OperationError => return "Operation Error";
            case cookieEcho: CookieEcho => return "Cookie Echo";
            case cookieAcknowledgement: CookieAcknowledgement => return "Cookie Acknowledgement";
            case ecnEcho: EcnEcho => return "Ecn Echo";
            case cwr: Cwr => return "Cwr";
            case shutdownComplete: ShutdownComplete => return "Shutdown Complete";
            default                                                  => return "Unknown Chunk Type";
        }
    }

}
