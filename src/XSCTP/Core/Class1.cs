using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{

    //
    //readonly ref struct ReadOnlySpan<T>

    ref struct BIO
    {
        public Span<byte> source;

        private int CurPos;

        public static (bool, int size) AvailableRead<T>(Span<byte> span, ref int curPos)
        {
            var size = Unsafe.SizeOf<T>();
            if (size > (span.Length - curPos))
            {
                return (false, size);
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            }
            return (true, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>() where T : unmanaged
        {
            var (ok, size) = AvailableRead<T>(source, ref CurPos);
            if (ok)
            {
                ref var v = ref Unsafe.As<byte, T>(ref source.GetPinnableReference());
                return ref v;
            }

        }


        public ref T ReadUnaligned<T>() where T : struct
        {
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference<byte>(Fff));
        }

    }
}
