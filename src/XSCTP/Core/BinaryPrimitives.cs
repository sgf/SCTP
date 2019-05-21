using System;
using System.Collections.Generic;
using System.Text;

namespace XSCTP.Core
{

    /// <summary>
    /// Reads bytes as primitives with specific endianness
    /// </summary>
    /// <remarks>
    /// For native formats, MemoryExtensions.Read{T}; should be used.
    /// Use these helpers when you need to read specific endinanness.
    /// </remarks>
    public static partial class BinaryPrimitives
    {
        /// <summary>
        /// This is a no-op and added only for consistency.
        /// This allows the caller to read a struct of numeric primitives and reverse each field
        /// rather than having to skip sbyte fields.
        /// </summary> 
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReverseEndianness(sbyte value)
        {
            return value;
        }

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReverseEndianness(short value) => (short)ReverseEndianness((ushort)value);

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReverseEndianness(int value) => (int)ReverseEndianness((uint)value);

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReverseEndianness(long value) => (long)ReverseEndianness((ulong)value);

        /// <summary>
        /// This is a no-op and added only for consistency.
        /// This allows the caller to read a struct of numeric primitives and reverse each field
        /// rather than having to skip byte fields.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReverseEndianness(byte value)
        {
            return value;
        }

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReverseEndianness(ushort value)
        {
            // Don't need to AND with 0xFF00 or 0x00FF since the final
            // cast back to ushort will clear out all bits above [ 15 .. 00 ].
            // This is normally implemented via "movzx eax, ax" on the return.
            // Alternatively, the compiler could elide the movzx instruction
            // entirely if it knows the caller is only going to access "ax"
            // instead of "eax" / "rax" when the function returns.

            return (ushort)((value >> 8) + (value << 8));
        }

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReverseEndianness(uint value)
        {
            // This takes advantage of the fact that the JIT can detect
            // ROL32 / ROR32 patterns and output the correct intrinsic.
            //
            // Input: value = [ ww xx yy zz ]
            //
            // First line generates : [ ww xx yy zz ]
            //                      & [ 00 FF 00 FF ]
            //                      = [ 00 xx 00 zz ]
            //             ROR32(8) = [ zz 00 xx 00 ]
            //
            // Second line generates: [ ww xx yy zz ]
            //                      & [ FF 00 FF 00 ]
            //                      = [ ww 00 yy 00 ]
            //             ROL32(8) = [ 00 yy 00 ww ]
            //
            //                (sum) = [ zz yy xx ww ]
            //
            // Testing shows that throughput increases if the AND
            // is performed before the ROL / ROR.

            return BitOperations.RotateRight(value & 0x00FF00FFu, 8) // xx zz
                + BitOperations.RotateLeft(value & 0xFF00FF00u, 8); // ww yy
        }

        /// <summary>
        /// Reverses a primitive value - performs an endianness swap
        /// </summary> 
        [CLSCompliant(false)]
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReverseEndianness(ulong value)
        {
            // Operations on 32-bit values have higher throughput than
            // operations on 64-bit values, so decompose.

            return ((ulong)ReverseEndianness((uint)value) << 32)
                + ReverseEndianness((uint)(value >> 32));
        }
    }

    public static partial class BinaryPrimitives
    {
        /// <summary>
        /// Reads an Int16 out of a read-only span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
        {
            short result = MemoryMarshal.Read<short>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads an Int32 out of a read-only span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
        {
            int result = MemoryMarshal.Read<int>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads an Int64 out of a read-only span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
        {
            long result = MemoryMarshal.Read<long>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads a UInt16 out of a read-only span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
        {
            ushort result = MemoryMarshal.Read<ushort>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads a UInt32 out of a read-only span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
        {
            uint result = MemoryMarshal.Read<uint>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads a UInt64 out of a read-only span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
        {
            ulong result = MemoryMarshal.Read<ulong>(source);
            if (BitConverter.IsLittleEndian)
            {
                result = ReverseEndianness(result);
            }
            return result;
        }

        /// <summary>
        /// Reads an Int16 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain an Int16, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt16BigEndian(ReadOnlySpan<byte> source, out short value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }

        /// <summary>
        /// Reads an Int32 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain an Int32, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt32BigEndian(ReadOnlySpan<byte> source, out int value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }

        /// <summary>
        /// Reads an Int64 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain an Int64, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt64BigEndian(ReadOnlySpan<byte> source, out long value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }

        /// <summary>
        /// Reads a UInt16 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain a UInt16, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt16BigEndian(ReadOnlySpan<byte> source, out ushort value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }

        /// <summary>
        /// Reads a UInt32 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain a UInt32, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt32BigEndian(ReadOnlySpan<byte> source, out uint value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }

        /// <summary>
        /// Reads a UInt64 out of a read-only span of bytes as big endian.
        /// <returns>If the span is too small to contain a UInt64, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt64BigEndian(ReadOnlySpan<byte> source, out ulong value)
        {
            bool success = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return success;
        }
    }

    public static partial class BinaryPrimitives
    {
        /// <summary>
        /// Writes an Int16 into a span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BigEndian(Span<byte> destination, short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes an Int32 into a span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BigEndian(Span<byte> destination, int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes an Int64 into a span of bytes as big endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BigEndian(Span<byte> destination, long value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Write a UInt16 into a span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Write a UInt32 into a span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Write a UInt64 into a span of bytes as big endian.
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes an Int16 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt16BigEndian(Span<byte> destination, short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes an Int32 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt32BigEndian(Span<byte> destination, int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes an Int64 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64BigEndian(Span<byte> destination, long value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Write a UInt16 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt16BigEndian(Span<byte> destination, ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Write a UInt32 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt32BigEndian(Span<byte> destination, uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Write a UInt64 into a span of bytes as big endian.
        /// <returns>If the span is too small to contain the value, return false.</returns>
        /// </summary>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt64BigEndian(Span<byte> destination, ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                value = ReverseEndianness(value);
            }
            return MemoryMarshal.TryWrite(destination, ref value);
        }
    }


}
