using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Pack
{



    pattern ChunkValue = PayloadData | // 0
        Initiation | // 1
        InitiationAcknowledgement | // 2
        SelectiveAcknowledgement | // 3
        HeartbeatRequest | // 4
        HeartbeatAcknowledgement | // 5
        AbortAssociation | // 6
        ShutdownAssociation | // 7
        ShutdownAcknowledgement | // 8
        OperationError | // 9
        CookieEcho | // 10
        CookieAcknowledgement | // 11
        EcnEcho | // 12
        Cwr | // 13
        ShutdownComplete;              // 14

    /********************************************** Chunk Types **********************************************/
    type PayloadData // 0
    {
    byte Type where value == 0;
    byte Reserved with BinaryEncoding { Width = 5};
    byte U with BinaryEncoding { Width = 1};
    byte B with BinaryEncoding { Width = 1};
    byte E with BinaryEncoding { Width = 1};
    ushort Length;
    uint TSN;
    ushort StreamIdentifier;
    ushort StreamSequenceNumber;
    uint PayloadProtocolIdentifier;
        binary UserData with BinaryEncoding { Length = Length - 16};
    }

    type Initiation // 1
    {
    byte Type where value == 1;
    byte ChunkFlags;
    ushort ChunkLength;
    uint InitiateTag;
    uint AdvertisedReceiverWindowCredit;
    ushort NumberofOutboundStreams;
    ushort NumberofInboundStreams;
    uint InitialTSN;
        optional [|ChunkLength > 20|]
        array<InitOptionalOrVariableParameter> OptionalOrVariableParameters;
    }


    pattern InitOptionalOrVariableParameter = IPv4AddressParameter | // 5
        IPv6AddressParameter | // 6
        CookiePreservative | // 9
        HostNameAddress | // 11
        SupportedAddressTypes | // 12
        EcnParameter;                                                // 32768

    class IPv4AddressParameter
    {
        ushort Type = 5;
        ushort Length = 8;
        IPv4Address IPv4Address;
    }

    class IPv6AddressParameter
    {
        ushort Type = 6;
        ushort Length = 20;
        IPv6Address IPv6Address;
    }

    class CookiePreservative
    {
        ushort Type = 9;
        ushort Length = 8;
        uint SuggestedCookieLifeSpanIncrement;
    }

    class HostNameAddress
    {
        ushort Type = 11;
        ushort Length;
        string HostName with BinaryEncoding { Length = Length - 4,TextEncoding = TextEncoding.ASCII };
    }


    class SupportedAddressTypes
    {
        ushort Type=12;
        ushort Length;
        array<ushort> AddressTypes with BinaryEncoding { Length = ((Length - 4) / 2)};
}


class InitiationAcknowledgement // 2
{
    byte Type=2;
    byte ChunkFlags;
    ushort ChunkLength;
    uint InitiateTag;
    uint AdvertisedReceiverWindowCredit;
    ushort NumberOfOutboundStreams;
    ushort NumberOfInboundStreams;
    uint InitialTSN;
    array<InitAckOptionalOrVariableParameter> OptionalOrVariableParameters;
}

pattern InitAckOptionalOrVariableParameter = StateCookie | // 7
    IPv4AddressParameter | // 5
    IPv6AddressParameter | // 6
    UnrecognizedParameter | // 8
    HostNameAddress | // 11
    EcnParameter;                                          // 32768

class StateCookie
{
    ushort Type=7;
    ushort Length;
    binary Value with BinaryEncoding { Length = Length - 4};
}

class UnrecognizedParameter
{
    ushort Type=8;
    ushort Length;
    binary Value with BinaryEncoding { Length = Length - 4};
}

class EcnParameter
{
    ushort Type=32768;
    ushort Length=4;
}

class SelectiveAcknowledgement // 3
{
    byte Type=3;
    byte ChunkFlags;
    ushort ChunkLength;
    uint CumulativeTsnAck;
    uint AdvertisedReceiverWindowCredit;
    ushort NumberOfGapAckBlocks;
    ushort NumberOfDuplicateTsns;
    array<GapAckBlock> GapAckBlocks with BinaryEncoding { Length = NumberOfGapAckBlocks};
    array<uint> DuplicateTsns with BinaryEncoding { Length = NumberOfDuplicateTsns};
}

class GapAckBlock
{
    ushort Start;
    ushort End;
}

class HeartbeatRequest // 4
{
    byte Type=4;
    byte ChunkFlags;
    ushort HeartbeatLength;
    HeartbeatInformation HeartbeatInformationTlv;
}

class HeartbeatInformation
{
    ushort HeartbeatInfoType=1;
    ushort HBInfoLength;
    binary SenderSpecificHeartbeatInfo with BinaryEncoding { Length = HBInfoLength - 4};
}

class HeartbeatAcknowledgement // 5
{
    byte Type=5;
    byte ChunkFlags;
    ushort HeartbeatAckLength;
    HeartbeatInformation HeartbeatInformationTlv;
}

class AbortAssociation // 6
{
    byte Type=6;
    byte Reserved with BinaryEncoding { Width = 7};
    byte T with BinaryEncoding { Width = 1};
    ushort Length;
    optional [|Length > 4|]
    array<CauseOfError> ErrorCauses;
}

class ShutdownAssociation // 7
{
    byte Type=7;
    byte ChunkFlags;
    ushort Length=8;
    uint CumulativeTsnAck;
}

class ShutdownAcknowledgement // 8
{
    byte Type=8;
    byte ChunkFlags;
    ushort Length=4;
}

class OperationError // 9
{
    byte Type=9;
    byte ChunkFlags;
    ushort Length;
    array<CauseOfError> ErrorCauses;
}

pattern CauseOfError = InvalidStreamIdentifier | // 1
    MissingMandatoryParameter | // 2
    StaleCookieError | // 3
    OutOfResource | // 4
    UnresolvableAddress | // 5
    UnrecognizedChunkType | // 6
    InvalidMandatoryParameter | // 7
    UnrecognizedParameters | // 8
    NoUserData | // 9
    CookieReceivedWhileShuttingDown | // 10
    RestartOfAnAssociationWithNewAddresses | // 11
    UserInitiatedAbort | // 12
    ProtocolViolation;                           // 13

class InvalidStreamIdentifier
{
    ushort CauseCode=1;
    ushort CauseLength=8;
    ushort StreamIdentifier;
    ushort Reserved;
}

class MissingMandatoryParameter
{
    ushort CauseCode=2;
    ushort CauseLength;
    uint NumberOfMissingParams;
    array<ushort> MissingParams with BinaryEncoding { Length = NumberOfMissingParams};
}

class StaleCookieError
{
    ushort CauseCode=3;
    ushort CauseLength=8;
    uint MeasureOfStaleness;
}

class OutOfResource
{
    ushort CauseCode=4;
    ushort CauseLength=4;
}

class UnresolvableAddress
{
    ushort CauseCode=5;
    ushort CauseLength;
    UnresolvableAddressPattern UnresolvableAddress;
}

pattern UnresolvableAddressPattern = IPv4AddressParameter |
    IPv6AddressParameter |
    HostNameAddress;

class UnrecognizedChunkType
{
    ushort CauseCode=6;
    ushort CauseLength;
    binary UnrecognizedChunk with BinaryEncoding { Length = CauseLength - 4};
}

class InvalidMandatoryParameter
{
    ushort CauseCode=7;
    ushort CauseLength=4;
}

class UnrecognizedParameters
{
    ushort CauseCode=8;
    ushort CauseLength;
    array<UnrecognizedParameter> UnrecognizedParameters;
}

class NoUserData
{
    ushort CauseCode=9;
    ushort CauseLength=8;
    uint TsnValue;
}

class CookieReceivedWhileShuttingDown
{
    ushort CauseCode=10;
    ushort CauseLength=4;
}

class RestartOfAnAssociationWithNewAddresses
{
    ushort CauseCode=11;
    ushort CauseLength;
    array<NewAddressTlv> NewAddressTlvs;
}

pattern NewAddressTlv = IPv4AddressParameter | IPv6AddressParameter;

class UserInitiatedAbort
{
    ushort CauseCode=12;
    ushort CauseLength;
    binary UpperLayerAbortReason with BinaryEncoding { Length = CauseLength - 4};
}

class ProtocolViolation
{
    ushort CauseCode=13;
    ushort CauseLength;
    binary AdditionalInformation with BinaryEncoding { Length = CauseLength - 4};
}

class CookieEcho // 10
{
    byte Type=10;
    byte ChunkFlags;
    ushort Length;
    binary Cookie with BinaryEncoding { Length = Length - 4};
}

class CookieAcknowledgement // 11
{
    byte Type=11;
    byte ChunkFlags;
    ushort Length=4;
}

class ShutdownComplete // 14
{
    byte Type=14;
    byte Reserved with BinaryEncoding { Width = 7};
    byte T with BinaryEncoding { Width = 1};
    ushort Length=4;
}

class EcnEcho // 12
{
    byte Type=12;
    byte ChunkFlags=0;
    ushort Length=8;
    uint LowestTsnNumber;
}

class Cwr // 13
{
    byte Type=13;
    byte ChunkFlags=0;
    ushort Length=8;
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
