using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SSN = System.UInt32;//流顺序号 TSN 和SSN（流顺序号）是相互独立的，TSN 用于保证传输的可靠性，SSN 用于保证流内消息的顺序传递。
using TSN = System.UInt32;//发送顺序号（TSN） SCTP 在将数据（数据分片或未分片的用户数据报）发送给底层之前顺序地为 之分配一个发送顺序号（TSN）。
                          //TSN 和SSN 在功能上使可靠传递和顺序传递分开。接收端证实所有收到的 TSNs，即使其中有些尚未收到。
                          //包重发功能负责 TSN 的证实，还负责拥塞消除。

namespace NetCore.Pack
{

    unsafe class ErrorCause
    {
        public ErrorCause()
        {

        }

        public CauseCode Code { get { return Head.Code; } }
        public ushort Length { get { return Head.Length; } }
        public ErrorHead Head { get; private set; }
        public Memory<byte> BodyBuff;
        public ICauseBody Body { get; private set; }

        public static ErrorCause FromBuff(Memory<byte> buff)
        {
            var error = new ErrorCause();
            ref var head = ref buff.Read<ErrorHead>();
            var _buff = buff.Slice(sizeof(ErrorHead));
            switch (head.Code)
            {
                case CauseCode.OutOfResource:
                case CauseCode.InvalidMandatoryParameter:
                case CauseCode.CookieReceivedWhileShuttingDown:
                    //只有头没有别的因此 无须读取Body
                    break;
                case CauseCode.NoUserData:
                    error.Body = _buff.Read<Error_NoUserData>();
                    break;
                case CauseCode.StaleCookieError:
                    error.Body = _buff.Read<Error_StaleCookieError>();
                    break;
                case CauseCode.InvalidStreamIdentifier:
                    error.Body = _buff.Read<InvalidStreamIdentifier>();
                    break;
                default:
                    break;
            }

            return error;
        }


        public static readonly ErrorCause
            New_OutOfResource = new ErrorCause() { Head = ErrorHead.New_OutOfResource },
            New_InvalidMandatoryParameter = new ErrorCause() { Head = ErrorHead.New_InvalidMandatoryParameter },
            New_CookieReceivedWhileShuttingDown = new ErrorCause() { Head = ErrorHead.New_CookieReceivedWhileShuttingDown };

        public static ErrorCause New_NoUserData(ref TSN tsn)
        {
            var error = new ErrorCause();
            error.Head = ErrorHead.New_NoUserData;
            error.Body = new Error_NoUserData { TSN = tsn };
            return error;
        }

        public static ErrorCause New_StaleCookieError(uint measureOfStaleness)
        {
            var error = new ErrorCause();
            error.Head = ErrorHead.New_StaleCookieError;
            error.Body = new Error_StaleCookieError { MeasureOfStaleness = measureOfStaleness };
            return error;
        }

        public static ErrorCause New_InvalidStreamIdentifier(ushort streamIdentifier)
        {
            var error = new ErrorCause();
            error.Head = ErrorHead.New_InvalidStreamIdentifier;
            error.Body = new InvalidStreamIdentifier { StreamIdentifier = streamIdentifier };
            return error;

        }


        public static ErrorCause New_MissingMandatoryParameter(ushort[] listOfMissingParamType)
        {
            var len = (ushort)(8 + (uint)listOfMissingParamType.Length * 2);
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new MissingMandatoryParameter { MissingParamType_List = new List<ushort>(listOfMissingParamType), NumberOfMissingParams = (uint)listOfMissingParamType.Length };
            error.Head = ErrorHead.New(CauseCode.MissingMandatoryParameter, len);
            return error;
        }


        public static ErrorCause New_UnresolvableAddress(List<(ushort completeType, int Length, string address)> address_List)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnresolvableAddress { Address_List = address_List };
            error.Head = ErrorHead.New(CauseCode.UnresolvableAddress, len);
            return error;
        }

        public static ErrorCause New_UnrecognizedChunkType(List<(ushort Type, int Flags, string Length)> chunk_List)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);
            return error;
        }

        public static ErrorCause New_UnrecognizedParameters(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }

        public static ErrorCause New_CookieReceivedWhileShuttingDown(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }

        public static ErrorCause New_RestartOfAnAssociationWithNewAddresses(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }

        public static ErrorCause New_UserInitiatedAbort(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }
        public static ErrorCause New_ProtocolViolation(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }
    }


    interface ICauseBody
    {

    }

    class MissingMandatoryParameter : ICauseBody
    {
        public uint NumberOfMissingParams;
        public List<ushort> MissingParamType_List;
    }

    class UnresolvableAddressPattern {
        IPv4AddressParameter
            IPv6AddressParameter
            HostNameAddress
    }




    class UnresolvableAddress : ICauseBody
    {
        public List<(ushort completeType, int Length, string address)> Address_List;
    }
    class UnrecognizedChunkType : ICauseBody
    {
        public List<(ushort Type, int Flags, string Length)> Chunk_List;
    }


    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct InvalidStreamIdentifier : ICauseBody
    {
        [FieldOffset(0)]
        public ushort StreamIdentifier;
        [FieldOffset(4)]
        public ushort Reserved;
    }

    /// <summary>
    /// This cause code is normally returned in an ABORT chunk (see Section 6.2).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_NoUserData : ICauseBody
    {
        /// <summary>
        /// The TSN value field contains the TSN of the DATA chunk received with no user data field.
        /// TSN值字段包含 在没有收到用户数据的情况下接收的DATA块的TSN。
        /// </summary>
        [FieldOffset(0)]
        public TSN TSN;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_StaleCookieError : ICauseBody
    {
        [FieldOffset(0)]
        public uint MeasureOfStaleness;
    }

    /// <summary>
    /// 公共头
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct ErrorHead
    {
        public const uint _OutOfResource = (uint)CauseCode.OutOfResource << 16 | 4,
            _InvalidMandatoryParameter = (uint)CauseCode.InvalidMandatoryParameter << 16 | 4,
            _CookieReceivedWhileShuttingDown = (uint)CauseCode.CookieReceivedWhileShuttingDown << 16 | 4,
            _StaleCookieError = (uint)CauseCode.StaleCookieError << 16 | 8,
            _NoUserData = (uint)CauseCode.NoUserData << 16 | 8,
            _InvalidStreamIdentifier = (uint)CauseCode.InvalidStreamIdentifier << 16 | 8
            ;

        public static readonly ErrorHead
            New_OutOfResource = new ErrorHead() { Value = _OutOfResource },
            New_NoUserData = new ErrorHead() { Value = _NoUserData },
            New_StaleCookieError = new ErrorHead() { Value = _StaleCookieError },
            New_InvalidMandatoryParameter = new ErrorHead() { Value = _InvalidMandatoryParameter },
            New_CookieReceivedWhileShuttingDown = new ErrorHead() { Value = _CookieReceivedWhileShuttingDown },
            New_InvalidStreamIdentifier = new ErrorHead() { Value = _InvalidStreamIdentifier };
        
        public static ErrorHead New(CauseCode code, ushort length)
        {
            return new ErrorHead() { Code = code, Length = length };
        }

        [FieldOffset(0)]
        public uint Value;

        /// <summary>
        /// 错误号
        /// </summary>
        [FieldOffset(0)]
        public CauseCode Code;
        /// <summary>
        /// 长度
        /// </summary>
        [FieldOffset(2)]
        public ushort Length;
    }

    public enum CauseCode : ushort
    {
        InvalidStreamIdentifier = 1,
        MissingMandatoryParameter = 2,
        StaleCookieError = 3,
        OutOfResource = 4,
        UnresolvableAddress = 5,
        UnrecognizedChunkType = 6,
        InvalidMandatoryParameter = 7,
        UnrecognizedParameters = 8,
        NoUserData = 9,
        CookieReceivedWhileShuttingDown = 10,
        RestartOfAnAssociationWithNewAddresses = 11,
        UserInitiatedAbort = 12,
        ProtocolViolation = 13,
    }
}
