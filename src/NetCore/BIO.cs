using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Binary IO
    /// </summary>
    public static class BIO
    {
        public unsafe static ref T Read<T>(this Memory<byte> memory) where T : unmanaged
        {
            var _m = memory;
            var size = sizeof(T);
            if (size >= _m.Length)
                throw new Exception("offset out of the lenght (长度溢出)");
            var _mp = _m.Pin();
            //var _mp = _m.Slice(0, sizeof(T)).Pin();
            var p = (T*)(_mp.Pointer);
            return ref *p;
        }

        public unsafe static ref T Read<T>(this Span<byte> memory) where T : unmanaged
        {
            var p = (T*)(memory.GetPinnableReference());
            //MemoryMarshal.GetReference(memory);
            //fixed(byte* t = memory)
            //{

            //}
            return ref *p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory"></param>
        /// <param name="t"></param>
        /// <returns>T size</returns>
        public unsafe static int Write<T>(this Memory<byte> memory, ref T t) where T : unmanaged
        {
            var p = (T*)memory.Pin().Pointer;
            *p = t;
            return sizeof(T);
        }


        public unsafe static int Write<T>(this Span<byte> memory,ref T t) where T : unmanaged
        {
            //ref memory.GetPinnableReference()

            var p = (T*)(memory.GetPinnableReference());
            *p = t;
            return sizeof(T);
        }
    }
}
