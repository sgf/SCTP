using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace XSCTP
{


    //
    //readonly ref struct ReadOnlySpan<T>

    unsafe ref struct BIO
    {
        public BIO(Span<byte> src)
        {
            _src = src;
            CurPos = 0;
            _psrc = (byte*)src.GetPinnableReference();
        }

        public Span<byte> _src;
        private readonly byte* _psrc;
        private int CurPos;


        public static (bool, int size) ReadInc<T>(Span<byte> span, ref int curPos)
        {
            var size = Unsafe.SizeOf<T>();
            if (size > (span.Length - curPos))
            {
                curPos = +size;
                return (false, size);
            }
            return (true, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadInc<T>(out bool ok) where T : unmanaged
        {
            ok = false;
            var size = Unsafe.SizeOf<T>();
            if (size > (_src.Length - CurPos))
            {
                CurPos = +size;
                ok = true;
            }
            return ref Unsafe.As<byte, T>(ref _src.Slice(CurPos, size).GetPinnableReference());
        }

        public ref T ReadUnaligned<T>() where T : struct
        {
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference<byte>(Fff));
        }

    }
}
