using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers.Binary;
using System.Buffers;

namespace System
{


    public class AAA
    {

        AAA()
        {

            BinaryPrimitives.read


}


    }


    interface IPackRead
    {
        void Fetch();
        void Read();
    }


    interface IPackWrite
    {
        void WriteHead();
        void WriteBody();
    }







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

        public unsafe static string Read_ASCII(this Memory<byte> memory, int length)
        {
            return ASCIIEncoding.ASCII.GetString(new ReadOnlySpan<byte>(memory.Pin().Pointer, length));
        }

        public unsafe static Span<T> Read<T>(this Memory<byte> memory, uint count) where T : unmanaged
        {
            var _m = memory;
            var size = sizeof(T);
            if (size * count >= _m.Length)
                throw new Exception("offset out of the lenght (长度溢出)");
            var _mp = _m.Pin();
            //var _mp = _m.Slice(0, sizeof(T)).Pin();
            return new Span<T>(_mp.Pointer, (int)(size * count));
        }

        public unsafe static ref T Read<T>(this Span<byte> memory) where T : unmanaged
        {
            //return ref MemoryMarshal.Read<T>(memory);
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


        public unsafe static int Write<T>(this Span<byte> memory, ref T t) where T : unmanaged
        {
            //ref memory.GetPinnableReference()

            var p = (T*)(memory.GetPinnableReference());
            *p = t;
            return sizeof(T);
        }
    }
}
