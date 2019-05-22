using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{
    internal
#if !PROJECTN // readonly breaks codegen contract and asserts UTC
    readonly
#endif
    ref struct ByReference<T>
    {
        // CS0169: The private field '{blah}' is never used
#pragma warning disable 169
        private readonly IntPtr _value;
#pragma warning restore

        [Intrinsic] //JitIntrinsic
        public ByReference(ref T value)
        {
            // Implemented as a JIT intrinsic - This default implementation is for 
            // completeness and to provide a concrete error if called via reflection
            // or if intrinsic is missed.
            throw new PlatformNotSupportedException();
        }

        public ref T Value
        {
            [Intrinsic]
            get
            {
                // Implemented as a JIT intrinsic - This default implementation is for 
                // completeness and to provide a concrete error if called via reflection
                // or if the intrinsic is missed.
                throw new PlatformNotSupportedException();
            }
        }
    }


    //
    //readonly ref struct ReadOnlySpan<T>

    unsafe ref  struct BIO
    {
        public BIO(Span<byte> src)
        {
            _src = src;
            CurPos = 0;
            _psrc = (byte*)src.GetPinnableReference();
        }

        public Span<byte> _src;
        private readonly ByReference<T> _psrc;
        private int CurPos;

        public static (bool, int size) ReadInc<T>(Span<byte> span, ref int curPos)
        {
            var size = Unsafe.SizeOf<T>();
            if (size > (span.Length - curPos))
            {
                curPos = +size;
                return (false, size);
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            }
            return (true, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>() where T : unmanaged
        {
            var (ok, size) = ReadInc<T>(_src, ref CurPos);
            if (ok)
            {
                ref var v = ref Unsafe.As<byte, T>(ref _src.Slice(CurPos, size).GetPinnableReference());
                return ref v;
            }
            return ref default(T);
        }


        public ref T ReadUnaligned<T>() where T : struct
        {
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference<byte>(Fff));
        }

    }
}
