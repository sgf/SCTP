using System;
using System.Collections.Generic;
using System.Text;

namespace XSCTP
{
    //Head of SCTP
    struct Head_SCTP
    {
        public ushort SrcPort;
        public ushort DstPort;
        //verification tag.
        public uint VerTag;
        //checksum value.
        public uint Chksum;
    }

    //Head of Chunk
    struct Head_Chunk
    {
        public ChunkType Type;
        public byte Flags;
        public ushort Length;
    }

    unsafe class SCTPPack
    {
        public SCTPPack(Head_SCTP* head)
        {
            Head = head;
        }
        public Head_SCTP* Head;

        public IChunk[] Chunks { get; set; }

    }


    interface IChunk
    {
        Head_Chunk Head { get; set; }

    }

    unsafe class Connect
    {

    }
}
