//TSN 和SSN 在功能上使可靠传递和顺序传递分开。接收端证实所有收到的 TSNs，即使其中有些尚未收到。
//包重发功能负责 TSN 的证实，还负责拥塞消除。
using SCTP;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SSN = System.UInt32;//流顺序号 TSN 和SSN（流顺序号）是相互独立的，TSN 用于保证传输的可靠性，SSN 用于保证流内消息的顺序传递。
using TSN = System.UInt32;//发送顺序号（TSN） SCTP 在将数据（数据分片或未分片的用户数据报）发送给底层之前顺序地为 之分配一个发送顺序号（TSN）。

namespace XSCTP
{


    unsafe class ErrorCause
    {

        /// <summary>
        /// Head Only
        /// </summary>
        /// <param name="head"></param>
        private ErrorCause(ErrorHead head)
        {
            Head = head;
        }

        /// <summary>
        ///  Length in Head is Fixed
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        private ErrorCause(ErrorHead head, ICauseBody body)
        {
            Body = body;
            Head = head;
        }

        /// <summary>
        /// Length in Head is var
        /// </summary>
        /// <param name="code"></param>
        /// <param name="body"></param>
        private ErrorCause(CauseCode code, ICauseBody body)
        {
            Body = body;
            Head = code.NewHead(body.Length);
        }

        public ErrorHead Head { get; private set; }


        public Memory<byte> BodyBuff;
        public ICauseBody Body { get; private set; }


        public void ReadBody(Memory<byte> buff)
        {
            switch (Head.Code)
            {
                case CauseCode.OutOfResource:
                case CauseCode.InvalidMandatoryParameter:
                case CauseCode.CookieReceivedWhileShuttingDown:
                    //只有头没有别的因此 无须读取Body
                    break;
                case CauseCode.NoUserData:
                    Body = buff.Read<Error_NoUserData>();
                    break;
                case CauseCode.StaleCookieError:
                    Body = buff.Read<Error_StaleCookieError>();
                    break;
                case CauseCode.InvalidStreamIdentifier:
                    Body = buff.Read<Error_InvalidStreamIdentifier>();
                    break;
                case CauseCode.MissingMandatoryParameter:
                    var numberOfMissingParams = buff.Read<uint>();
                    var missingParams = buff.Read<ushort>(numberOfMissingParams).ToArray();
                    Body = new Error_MissingMandatoryParameter(numberOfMissingParams, missingParams);
                    break;
                case CauseCode.RestartOfAnAssociationWithNewAddresses:
                    Body = RestartOfAnAssociationWithNewAddresses.Read(buff);
                    break;
                case CauseCode.ProtocolViolation:
                    Body = new ProtocolViolation(buff.Read<byte>(Head.Length).ToArray());
                    break;
                case CauseCode.UnrecognizedChunkType:
                    Body = buff.Read<UnrecognizedChunkType>();
                    break;
                case CauseCode.UnrecognizedParameters:
                    var unrecognizedParameters = buff.Read<byte>((uint)(Head.Length - sizeof(ErrorHead))).ToArray();
                    Body = new UnrecognizedParameters(unrecognizedParameters);
                    break;
                case CauseCode.UnresolvableAddress:
                    IUnresolvableAddress
                    Body = buff.Read<UnresolvableAddress>();
                    UnresolvableAddress unresolvableAddress = new UnresolvableAddress();
                    break;
                case CauseCode.UserInitiatedAbort:
                    Body = buff.Read<UserInitiatedAbort>();
                    break;

                default:
                    break;
            }
        }

        public static ErrorCause FromBuff(Memory<byte> buff)
        {
            ref var head = ref buff.Read<ErrorHead>();
            var error = new ErrorCause(head);
            var _buff = buff.Slice(sizeof(ErrorHead));

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
            new ErrorCause(CauseCode.MissingMandatoryParameter,
                new Error_MissingMandatoryParameter { MissingParams = listOfMissingParamType, NumberOfMissingParams = (uint)listOfMissingParamType.Length });
        public static ErrorCause New_UnresolvableAddress(IUnresolvableAddress address)
        {
            return new ErrorCause(CauseCode.UnresolvableAddress, new UnresolvableAddress { Address = address });
        }

        public static ErrorCause New_UnrecognizedChunkType(Head_Chunk chunk) => new ErrorCause(ErrorHead.New(CauseCode.UnrecognizedChunkType, (ushort)(4 + sizeof(Head_Chunk))), new UnrecognizedChunkType { Chunk = chunk });

        public static ErrorCause New_UnrecognizedParameters(byte[] val)
        {
            var up = new UnrecognizedParameters() { value = val };
            return new ErrorCause(ErrorHead.New(CauseCode.UnrecognizedChunkType, (ushort)(up.value.Length + sizeof(ErrorHead))), up);
        }

        public static ErrorCause New_RestartOfAnAssociationWithNewAddresses(ushort[] listOfMissingParamType)
        {
            var roaawna = new RestartOfAnAssociationWithNewAddresses();
            return new ErrorCause(ErrorHead.New(CauseCode.RestartOfAnAssociationWithNewAddresses, (ushort)(roaawna.Length + sizeof(ErrorHead))), roaawna);
        }

        public static ErrorCause New_UserInitiatedAbort(ushort[] listOfMissingParamType)
        {


            var uia = new UserInitiatedAbort() { UpperLayerAbortReason }
            return new ErrorCause(ErrorHead.New(CauseCode.UserInitiatedAbort, len);)
        }
        public static ErrorCause New_ProtocolViolation(ushort[] listOfMissingParamType)
        {
            var len =?;
            if (len > ushort.MaxValue) throw new Exception("超出ErrorCause.Length 的最大长度限制");
            var error = new ErrorCause();
            error.Body = new UnrecognizedChunkType { Chunk_List = chunk_List };
            error.Head = ;

            return new ErrorCause(ErrorHead.New(CauseCode.ProtocolViolation, len), new ProtocolViolation() { AdditionalInformation = new byte[0] });

        }
    }


    interface ICauseBody
    {
        ushort Length { get; }

    }



    interface NewAddressTlv
    {
        AddressHead Head { get; }
        AddressType Type => Head.Type;
        ushort Length => Head.Length;
    }

    class UserInitiatedAbort : ICauseBody
    {
        /// <summary>
        /// 上层放弃的原因（SCTP RFC中指出由上层协议定义）
        /// </summary>
        public byte[] UpperLayerAbortReason;//binary UpperLayerAbortReason with BinaryEncoding { Length = CauseLength - 4};

        public ushort Length => (ushort)UpperLayerAbortReason.Length;
    }

    /// <summary>
    /// 协议非法/协议无效/违反协议
    /// </summary>
    struct ProtocolViolation : ICauseBody
    {
        public ProtocolViolation(byte[] additionalInfo)
        {
            AdditionalInformation = additionalInfo;
        }

        /// <summary>
        /// 附加信息(可以提供违反了什么协议，或者协议哪些方面) An implementation MAY provide additional information specifying what kind of protocol violation has been detected.
        /// </summary>
        public byte[] AdditionalInformation;//binary AdditionalInformation with BinaryEncoding { Length = CauseLength - 4};

        public ushort Length => (ushort)AdditionalInformation.Length;

    }

    struct RestartOfAnAssociationWithNewAddresses : ICauseBody
    {
        public NewAddressTlv[] NewAddressTlvs;

        public ushort Length
        {
            get
            {
                int len = 0;
                foreach (var addr in NewAddressTlvs)
                {
                    len = len + addr.Length;
                }
                return (ushort)len;
            }
        }

        public static RestartOfAnAssociationWithNewAddresses Read(Memory<byte> buff)
        {
            List<NewAddressTlv> newAddressTlvs = new List<NewAddressTlv>();
            RestartOfAnAssociationWithNewAddresses roaawna;
            //roaawna.NewAddressTlvs = new NewAddressTlv[];
            //roaawna.NewAddressTlvs
            var pos = 0;
            while (pos < buff.Length)
            {
                var head = buff.Read<AddressHead>();
                switch (head.Type)
                {
                    case AddressType.V4://IPv4Address
                        newAddressTlvs.Add(new IPv4AddressParameter(buff.Read<IP4Address>()));
                        break;
                    case AddressType.V6:
                        newAddressTlvs.Add(new IPv6AddressParameter(buff.Read<IP6Address>()));
                        break;
                    case AddressType.HostName:
                        newAddressTlvs.Add(new HostNameAddressParameter(buff.Read_ASCII(head.Length)));
                        break;
                    default:
                        throw new Exception("不支持的地址或域名");
                }
            }
            roaawna.NewAddressTlvs = newAddressTlvs.ToArray();
            return roaawna;
        }


    }

    /// <summary>
    /// 无法识别的参数
    /// Use In :when the sender of the COOKIE ECHO chunk wishes to report unrecognized parameters.
    /// What:not recognize one or more Optional TLV parameters in the INIT ACK chunk.
    /// </summary>
    class UnrecognizedParameters : InitAckOptionalOrVariableParameter, ICauseBody
    {

        public UnrecognizedParameters(byte[] _uparms)
        {
            value = _uparms;
        }

        /// <summary>
        /// not recognize [one or more Optional TLV parameters] in the INIT ACK chunk.
        /// </summary>

        public byte[] value; //binary Value with BinaryEncoding { Length = Length - 4};

        public ushort Length => (ushort)value.Length;
    }

    struct Error_MissingMandatoryParameter : ICauseBody
    {
        public Error_MissingMandatoryParameter(uint numberOfMissingParams, ushort[] missingParams)
        {
            NumberOfMissingParams = numberOfMissingParams;
            MissingParams = missingParams;
        }

        public uint NumberOfMissingParams;
        public ushort[] MissingParams;
        public ushort Length => (ushort)(4 + MissingParams.Length * sizeof(ushort));
    }

    /// <summary>
    /// 无法解析的IP地址或主机域名
    /// </summary>
    class UnresolvableAddress : ICauseBody
    {
        public UnresolvableAddress(IUnresolvableAddress address)
        {
            Address = address;
            switch (address)
            {
                case IPv4AddressParameter ipv4:
                    Length = ipv4.Head.Length;
                    break;
                case IPv6AddressParameter ipv6:
                    Length = ipv6.Head.Length;
                    break;
                case HostNameAddressParameter host:
                    Length = host.Head.Length;
                    break;
                default:
                    throw new Exception("参数错误");
            }
        }

        public IUnresolvableAddress Address;
        public ushort Length { get; }
    }

    /// <summary>
    /// 未知的 Chunk 类型
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct UnrecognizedChunkType : ICauseBody
    {
        public UnrecognizedChunkType(Head_Chunk chunk)
        {
            Chunk = chunk;
        }
#warning 这里Chunk 类型为 Head_Chunk 未必正确
        public Head_Chunk Chunk;

        public ushort Length => (ushort)sizeof(Head_Chunk);
    }

    /// <summary>
    /// 无效的流Id
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_InvalidStreamIdentifier : ICauseBody
    {
        public Error_InvalidStreamIdentifier(ushort streamIdentifier, ushort reserved)
        {
            StreamIdentifier = streamIdentifier;
            Reserved = reserved;
        }

        [FieldOffset(0)]
        public ushort StreamIdentifier;
        [FieldOffset(2)]
        public ushort Reserved;

        public ushort Length => 4;
    }

    /// <summary>
    /// This cause code is normally returned in an ABORT chunk (see Section 6.2).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_NoUserData : ICauseBody
    {
        public Error_NoUserData(TSN tsn)
        {
            TSN = tsn;
        }

        /// <summary>
        /// The TSN value field contains the TSN of the DATA chunk received with no user data field.
        /// TSN值字段包含 在没有收到用户数据的情况下接收的DATA块的TSN。
        /// </summary>
        [FieldOffset(0)]
        public TSN TSN;

        public ushort Length => 4;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_StaleCookieError : ICauseBody
    {
        [FieldOffset(0)]
        public uint MeasureOfStaleness;

        public ushort Length => 4;
    }

    /// <summary>
    /// 公共头
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public struct ErrorHead
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
