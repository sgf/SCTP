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
        private ErrorCause(ErrorHead head)
        {
            Head = head;
        }
        private ErrorCause(ErrorHead head, ICauseBody body)
        {
            Head = head;
            Body = body;
        }

        public ErrorHead Head { get; private set; }
        public Memory<byte> BodyBuff;
        public ICauseBody Body { get; private set; }

        public static ErrorCause FromBuff(Memory<byte> buff)
        {
            ref var head = ref buff.Read<ErrorHead>();
            var error = new ErrorCause(head);
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
                    error.Body = _buff.Read<Error_InvalidStreamIdentifier>();
                    break;
                default:
                    break;
            }

            return error;
        }


        public static readonly ErrorCause
            New_OutOfResource = new ErrorCause(ErrorHead.New_OutOfResource),
            New_InvalidMandatoryParameter = new ErrorCause(ErrorHead.New_InvalidMandatoryParameter),
            New_CookieReceivedWhileShuttingDown = new ErrorCause(ErrorHead.New_CookieReceivedWhileShuttingDown);

        public static ErrorCause New_NoUserData(ref TSN tsn) => new ErrorCause(ErrorHead.New_NoUserData, new Error_NoUserData { TSN = tsn });
        public static ErrorCause New_StaleCookieError(uint measureOfStaleness) => new ErrorCause(ErrorHead.New_StaleCookieError, new Error_StaleCookieError { MeasureOfStaleness = measureOfStaleness });
        public static ErrorCause New_InvalidStreamIdentifier(ushort streamIdentifier) => new ErrorCause(ErrorHead.New_InvalidStreamIdentifier, new Error_InvalidStreamIdentifier { StreamIdentifier = streamIdentifier });
        public static ErrorCause New_MissingMandatoryParameter(params ushort[] listOfMissingParamType) =>
            new ErrorCause(ErrorHead.New(CauseCode.MissingMandatoryParameter, (ushort)(8 + (uint)listOfMissingParamType.Length * 2)),
                new Error_MissingMandatoryParameter { MissingParams = listOfMissingParamType, NumberOfMissingParams = (uint)listOfMissingParamType.Length });
        public static ErrorCause New_UnresolvableAddress(IUnresolvableAddress address)
        {
            ushort subLen = 0;
            switch (address)
            {
                case IPv4AddressParameter ipv4:
                    subLen = ipv4.Head.Length;
                    break;
                case IPv6AddressParameter ipv6:
                    subLen = ipv6.Head.Length;
                    break;
                case HostNameAddress host:
                    subLen = host.Head.Length;
                    break;
                default:
                    throw new Exception("参数错误");
            }
            return new ErrorCause(ErrorHead.New(CauseCode.UnresolvableAddress, subLen), new UnresolvableAddress { Address = address });
        }

        public static ErrorCause New_UnrecognizedChunkType(Head_Chunk chunk)=> new ErrorCause(ErrorHead.New(CauseCode.UnrecognizedChunkType, (ushort)(4 + sizeof(Head_Chunk))), new UnrecognizedChunkType { Chunk = chunk });

        public static ErrorCause New_UnrecognizedParameters(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedParameter { Chunk_List = chunk_List };
            error.Head = ErrorHead.New(CauseCode.UnrecognizedChunkType, len);

        }

        public static ErrorCause New_RestartOfAnAssociationWithNewAddresses(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new RestartOfAnAssociationWithNewAddresses {   = chunk_List };
            error.Head = ErrorHead.New(CauseCode.RestartOfAnAssociationWithNewAddresses, len);

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

    class Error_MissingMandatoryParameter : ICauseBody
    {
        public uint NumberOfMissingParams;
        public ushort[] MissingParams;
    }

    /// <summary>
    /// 无法解析的IP地址或主机域名
    /// </summary>
    class UnresolvableAddress : ICauseBody
    {
        public IUnresolvableAddress Address;

    }

    /// <summary>
    /// 未知的 Chunk 类型
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct UnrecognizedChunkType : ICauseBody
    {
        public Head_Chunk Chunk;
#warning 这里Chunk 类型为 Head_Chunk 未必正确
    }

    /// <summary>
    /// 无效的流Id
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_InvalidStreamIdentifier : ICauseBody
    {
        [FieldOffset(0)]
        public ushort StreamIdentifier;
        [FieldOffset(2)]
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
