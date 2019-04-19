namespace SCTP
{
    using System;
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
    /// This cause code is normally returned in an ABORT chunk (see Section 6.2).
    /// </summary>
    [StructLayout(LayoutKind.Explicit,Pack =1)]
    struct Error_NoUserData
    {
        public const ulong _Base = (ulong)CauseCode.NoUserData << 24 | 8 << 16;

        [FieldOffset(0)]
        public ulong _Value;

        [FieldOffset(0)]
        public CauseCode Code;
        [FieldOffset(2)]
        public sbyte Length;
        /// <summary>
        /// The TSN value field contains the TSN of the DATA chunk received with no user data field.
        /// TSN值字段包含 在没有收到用户数据的情况下接收的DATA块的TSN。
        /// </summary>
        [FieldOffset(4)]
        public TSN TSN;
        
        public static Error_NoUserData New(TSN tsn)
        {
            return new Error_NoUserData { _Value = _Base | tsn };
        }
    }

    struct StaleCookieError
    {
        public const CauseCode _Code = CauseCode.StaleCookieError;
        public const sbyte _Length = 8;
        public const ulong _Base = (ulong)_Code << 24 | _Length << 16;

        /// <summary>
        /// This field contains the difference, in microseconds, between the
        /// current time and the time the State Cookie expired.
        /// The sender of this error cause MAY choose to report how long past
        /// expiration the State Cookie is by including a non-zero value in
        /// the Measure of Staleness field.If the sender does not wish to
        /// provide this information, it should set the Measure of Staleness
        /// field to the value of zero.
        /// </summary>
        public uint MeasureOfStaleness;

    }

    struct OutOfResource
    {
        public const CauseCode _Code = CauseCode.OutOfResource;
        public const sbyte _Length = 4;
        public const uint _Base = (uint)_Code << 24 | _Length << 16;
    }

    struct InvalidMandatoryParameter
    {
        public const CauseCode _Code = CauseCode.InvalidMandatoryParameter;
        public const sbyte _Length = 4;
        public const uint _Base = (uint)_Code << 24 | _Length << 16;
    }

    struct CookieReceivedWhileShuttingDown
    {
        public const CauseCode _Code = CauseCode.CookieReceivedWhileShuttingDown;
        public const sbyte _Length = 4;
        public const uint _Base = (uint)_Code << 24 | _Length << 16;
    }


    struct UserInitiatedAbortV
    {

    }

    struct ProtocolViolationV
    {


    }

    struct RestartOfAnAssociationWithNewAddressesV
    {
        public const CauseCode _Code = CauseCode.RestartOfAnAssociationWithNewAddresses;
        public const sbyte _Length = 4;
        public const ulong Error_NoUserDataBase = (ushort)_Code << 24 | _Length << 16;
    }



    /// <summary>
    /// 动态长度
    /// </summary>
    struct UnresolvableAddressV
    {
        public const CauseCode _Code = CauseCode.UnresolvableAddress;
        public const sbyte _Length = 4;
        public const ulong Error_NoUserDataBase = (ushort)_Code << 24 | _Length << 16;
    }




    /// <summary>
    /// 动态长度
    /// </summary>
    struct UnrecognizedChunkTypeV
    {

    }


    /// <summary>
    /// 动态长度
    /// </summary>
    struct MissingMandatoryParameterV
    {

    }




    /// <summary>
    /// 动态长度
    /// </summary>
    struct UnrecognizedParametersV
    {
        public const CauseCode _Code = CauseCode.InvalidMandatoryParameter;
        public const sbyte _Length = 4;
        public const ulong Error_NoUserDataBase = (ushort)_Code << 24 | _Length << 16;
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

    public class ErrorCauseM
    {

        public ErrorCause New(CauseCode code)
        {

            var ec = new ErrorCause(code);

            switch (code)
            {
                case CauseCode.NoUserData:



                    break;
                    ;




            }




        }
        switch()






    }

}
