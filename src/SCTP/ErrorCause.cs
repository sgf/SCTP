namespace SCTP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using SSN = System.UInt32;//流顺序号 TSN 和SSN（流顺序号）是相互独立的，TSN 用于保证传输的可靠性，SSN 用于保证流内消息的顺序传递。
    using TSN = System.UInt32;//发送顺序号（TSN） SCTP 在将数据（数据分片或未分片的用户数据报）发送给底层之前顺序地为 之分配一个发送顺序号（TSN）。
    //TSN 和SSN 在功能上使可靠传递和顺序传递分开。接收端证实所有收到的 TSNs，即使其中有些尚未收到。
    //包重发功能负责 TSN 的证实，还负责拥塞消除。




    /// <summary>
    /// 
    /// Represents an error cause.
    /// </summary>
    public abstract class ErrorCause
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ErrorCause"/> class.
        /// </summary>
        /// <param name="code"></param>
        protected ErrorCause(CauseCode code)
        {
            this.Code = code;
        }



        /// <summary>
        /// Gets the error cause code.
        /// </summary>
        public CauseCode Code { get; private set; }

        /// <summary>
        /// Gets the length of the error cause.
        /// </summary>
        internal ushort Length { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] buffer = new byte[this.Length];
            this.ToArray(buffer, 0);
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ToArray(byte[] buffer, int offset)
        {
            NetworkHelpers.CopyTo((ushort)this.Code, buffer, offset);
            int payloadLength = this.ToBuffer(buffer, offset + 4);
            this.Length = Convert.ToUInt16(payloadLength + 4);
            NetworkHelpers.CopyTo(this.Length, buffer, offset + 2);
            return this.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected abstract int ToBuffer(byte[] buffer, int offset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal int FromArray(byte[] buffer, int offset)
        {

            //BinaryReader reader = new BinaryReader(buffer);


            int start = offset;

            this.Code = (CauseCode)NetworkHelpers.ToUInt16(buffer, offset);
            offset += 2;
            this.Length = NetworkHelpers.ToUInt16(buffer, offset);
            offset += 2;
            offset += this.FromBuffer(buffer, offset);
            return offset - start; ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected abstract int FromBuffer(byte[] buffer, int offset);
    }



    /// <summary>
    /// 公共头
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct ErrorHead
    {

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

    #region fixed size Error Cause Define

    /// <summary>
    /// This cause code is normally returned in an ABORT chunk (see Section 6.2).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_NoUserData
    {
        public const ulong _Base = (ulong)CauseCode.NoUserData << 24 | 8 << 16;


        [FieldOffset(0)]
        public ulong Value;

        [FieldOffset(0)]
        public ErrorHead Head;
        /// <summary>
        /// The TSN value field contains the TSN of the DATA chunk received with no user data field.
        /// TSN值字段包含 在没有收到用户数据的情况下接收的DATA块的TSN。
        /// </summary>
        [FieldOffset(4)]
        public TSN TSN;

        public static Error_NoUserData New(TSN tsn)
        {
            return new Error_NoUserData { Value = _Base | tsn };
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_StaleCookieError
    {
        public const ulong _Base = (ulong)CauseCode.StaleCookieError << 24 | 8 << 16;


        [FieldOffset(0)]
        public ulong Value;

        [FieldOffset(0)]
        public ErrorHead Head;

        /// <summary>
        /// This field contains the difference, in microseconds, between the
        /// current time and the time the State Cookie expired.
        /// The sender of this error cause MAY choose to report how long past
        /// expiration the State Cookie is by including a non-zero value in
        /// the Measure of Staleness field.If the sender does not wish to
        /// provide this information, it should set the Measure of Staleness
        /// field to the value of zero.
        /// </summary>
        [FieldOffset(4)]
        public uint MeasureOfStaleness;

        public static Error_StaleCookieError New(uint measureOfStaleness)
        {
            return new Error_StaleCookieError { Value = _Base | measureOfStaleness };

        }


    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_OutOfResource
    {
        public const uint _Base = (uint)CauseCode.OutOfResource << 24 | 4 << 16;

        [FieldOffset(0)]
        public ulong Value;

        [FieldOffset(0)]
        public ErrorHead Head;

        public static Error_OutOfResource New()
        {
            return new Error_OutOfResource { Value = _Base };
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_InvalidMandatoryParameter
    {
        public const uint _Base = (uint)CauseCode.InvalidMandatoryParameter << 24 | 4 << 16;

        [FieldOffset(0)]
        public uint Value;

        [FieldOffset(0)]
        public ErrorHead Head;

        public static Error_InvalidMandatoryParameter New()
        {
            return new Error_InvalidMandatoryParameter { Value = _Base };
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_CookieReceivedWhileShuttingDown
    {
        public const uint _Base = (uint)CauseCode.CookieReceivedWhileShuttingDown << 24 | 4 << 16;

        [FieldOffset(0)]
        public uint Value;

        [FieldOffset(0)]
        public ErrorHead Head;

        public static Error_InvalidMandatoryParameter New()
        {
            return new Error_InvalidMandatoryParameter { Value = _Base };
        }
    }


    #endregion

    /// <summary>
    ///  User Initiated Abort 动态长度 但是这里暂时不启用UpperLayerAbortReason 因此变成固定长度
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    struct Error_UserInitiatedAbortV
    {
        public const uint _Base = (uint)CauseCode.UserInitiatedAbort << 24 | 4 << 16;

        [FieldOffset(0)]
        public uint Value;
        [FieldOffset(0)]
        public ErrorHead Head;
        //public List<UpperLayerAbortReason> Reasons;

        public static Error_UserInitiatedAbortV New()
        {
            //Error_UserInitiatedAbortV error;
            //error.Head.Code = CauseCode.UserInitiatedAbort;
            //error.Head.Length = ?;
            //error.Reasons = new List<UpperLayerAbortReason>();
            return new Error_UserInitiatedAbortV {Value= _Base };
        }

        [Obsolete("上层协议中止原因（上层协议中止传输的原因） 目前暂不启用，目前这块在RFC4960中定义 不够明确 似乎是需要上层协议来定义")]
        struct UpperLayerAbortReason
        {

        }
    }


    struct Error_ProtocolViolationV
    {
        public const uint _Base = (uint)CauseCode.ProtocolViolation << 24 | 4 << 16;


    }

    struct Error_RestartOfAnAssociationWithNewAddressesV
    {
        public const uint _Base = (uint)CauseCode.RestartOfAnAssociationWithNewAddresses << 24 | 4 << 16;
    }



    /// <summary>
    /// 动态长度
    /// </summary>
    struct Error_UnresolvableAddressV
    {
        public const uint _Base = (uint)CauseCode.UnresolvableAddress << 24 | 4 << 16;
    }

    /// <summary>
    /// 动态长度
    /// </summary>
    struct Error_UnrecognizedChunkTypeV
    {
        public const uint _Base = (uint)CauseCode.UnrecognizedChunkType << 24 | 4 << 16;

    }


    /// <summary>
    /// 动态长度
    /// </summary>
    struct Error_MissingMandatoryParameterV
    {
        public const uint _Base = (uint)CauseCode.MissingMandatoryParameter << 24 | 4 << 16;

    }

    /// <summary>
    /// 动态长度
    /// </summary>
    struct Error_UnrecognizedParametersV
    {
        public const uint _Base = (uint)CauseCode.UnrecognizedParameters << 24 | 4 << 16;
    }




    //    TSN: 32 bits(unsigned integer)
    //This value represents the TSN for this DATA chunk. The valid
    //range of TSN is from 0 to 4294967295 (2**32 - 1). TSN wraps back
    //to 0 after reaching 4294967295.

    //Stream Identifier S: 16 bits (unsigned integer)
    //Identifies the stream to which the following user data belongs.

    //Stream Sequence Number n: 16 bits (unsigned integer)
    //This value represents the Stream Sequence Number of the following
    //user data within the stream S. Valid range is 0 to 65535.

   

}
