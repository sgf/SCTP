using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers;
using bp = System.Buffers.Binary.BinaryPrimitives;
using WinDivertSharp;

namespace SCTP.Pack
{

    struct CommonHeader
    {
        public ushort SrcPort;
        public ushort DstPort;
        //verification tag.
        public uint VerTag;
        //checksum value.
        public uint Chksum;


        public static CommonHeader FromBig(Span<byte> buff)
        {
            CommonHeader ch;
            ch.SrcPort = bp.ReadUInt16BigEndian(buff.Slice(0));
            ch.DstPort = bp.ReadUInt16BigEndian(buff.Slice(2));
            ch.VerTag = bp.ReadUInt32BigEndian(buff.Slice(4));
            ch.Chksum = bp.ReadUInt32BigEndian(buff.Slice(6));
            return ch;
        }


        public void ToBig(Span<byte> buff)
        {
            bp.WriteUInt16BigEndian(buff.Slice(0), SrcPort);
            bp.WriteUInt16BigEndian(buff.Slice(2), DstPort);
            bp.WriteUInt32BigEndian(buff.Slice(4), VerTag);
            bp.WriteUInt32BigEndian(buff.Slice(6), Chksum);
        }
    }

    struct ChunkHead
    {
        public ChunkType Type;
        public byte Flags;
        public ushort Length;
    }

    ref struct SCTP_Pack
    {
        public CommonHeader Header;

    }

    enum ChunkType
       : byte
    {
        /// <summary>
        /// Payload data
        /// </summary>
        Data = 0,

        /// <summary>
        /// Initiation
        /// </summary>
        Init = 1,

        /// <summary>
        /// Initiation acknowledgement.
        /// </summary>
        InitAck = 2,

        /// <summary>
        /// Selective acknowledgement.
        /// </summary>
        Sack = 3,

        /// <summary>
        /// Heartbeat request.
        /// </summary>
        Heartbeat = 4,

        /// <summary>
        /// Heartbeat acnowledgement.
        /// </summary>
        HeartbeatAck = 5,

        /// <summary>
        /// Abort.
        /// </summary>
        Abort = 6,

        /// <summary>
        /// Shutdown.
        /// </summary>
        Shutdown = 7,

        /// <summary>
        /// Shutdown acknowledgement.
        /// </summary>
        ShutdownAck = 8,

        /// <summary>
        /// Operation error.
        /// </summary>
        Error = 9,

        /// <summary>
        /// State cookie.
        /// </summary>
        CookieEcho = 10,

        /// <summary>
        /// Cookie acknowledgement.
        /// </summary>
        CookieAck = 11,

        /// <summary>
        /// Reserved for Explicit Congestion Notification Echo (ECNE)
        /// </summary>
        ReservedEcne = 12,

        /// <summary>
        /// Reserved for Congestion Window Reduced (CWR)
        /// </summary>
        ReservesCwr = 13,

        /// <summary>
        /// Shutdown complete.
        /// </summary>
        ShutdownComplete = 14,

        AUTH = 15,
        PKTDROP = 129,
        RE_CONFIG = 130,
        FORWARDTSN = 192,
        ASCONF = 193,
        ASCONF_ACK = 128,
    }
}
