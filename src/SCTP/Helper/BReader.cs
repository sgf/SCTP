using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SCTP
{
    public static class IO
    {
        //
        public unsafe static ref T Read<T>(this ReadOnlyMemory<byte> memory) where T : unmanaged
        {
            var _m = memory;
            var size = sizeof(T);
            if (size >= _m.Length)
                throw new Exception("offset out of the lenght (长度溢出)");
            var _mp = _m.Slice(0, sizeof(T)).Pin();
            var p = (T*)(_mp.Pointer);
            return ref *p;
        }

        public unsafe static ref T Read<T>(this ReadOnlySpan<byte> memory) where T : unmanaged
        {
            var p = (T*)(memory.GetPinnableReference());
            //MemoryMarshal.GetReference(memory);
            //fixed(byte* t = memory)
            //{

            //}
            return ref *p;
        }

        public unsafe static ref T Read<T>(this ReadOnlySpan<byte> memory) where T : unmanaged
        {
            //ref memory.GetPinnableReference()
            new ReadOnlySpan()

            //
            var p = (T*)(memory.Pointer);
            return ref *p;
        }
    }
}
