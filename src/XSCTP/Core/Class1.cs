using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{
    internal class IPAddressParser
    {
        // Token: 0x06000122 RID: 290 RVA: 0x0000A7EC File Offset: 0x000091EC
        internal unsafe static IPAddress Parse(ReadOnlySpan<char> ipSpan, bool tryParse)
        {
            long newAddress;
            if (ipSpan.Contains(':'))
            {
                ushort* ptr = stackalloc ushort[(UIntPtr)16];
                new Span<ushort>((void*)ptr, 8).Clear();
                uint scopeid;
                if (IPAddressParser.Ipv6StringToAddress(ipSpan, ptr, 8, out scopeid))
                {
                    return new IPAddress(ptr, 8, scopeid);
                }
            }
            else if (IPAddressParser.Ipv4StringToAddress(ipSpan, out newAddress))
            {
                return new IPAddress(newAddress);
            }
            if (tryParse)
            {
                return null;
            }
            throw new FormatException(SR.dns_bad_ip_address, new SocketException(SocketError.InvalidArgument));
        }

        // Token: 0x06000123 RID: 291 RVA: 0x0000A858 File Offset: 0x00009258
        internal unsafe static string IPv4AddressToString(uint address)
        {
            char* ptr = stackalloc char[(UIntPtr)30];
            int length = IPAddressParser.IPv4AddressToStringHelper(address, ptr);
            return new string(ptr, 0, length);
        }

        // Token: 0x06000124 RID: 292 RVA: 0x0000A87C File Offset: 0x0000927C
        internal unsafe static void IPv4AddressToString(uint address, StringBuilder destination)
        {
            char* ptr = stackalloc char[(UIntPtr)30];
            int valueCount = IPAddressParser.IPv4AddressToStringHelper(address, ptr);
            destination.Append(ptr, valueCount);
        }

        // Token: 0x06000125 RID: 293 RVA: 0x0000A8A0 File Offset: 0x000092A0
        internal unsafe static bool IPv4AddressToString(uint address, Span<char> formatted, out int charsWritten)
        {
            if (formatted.Length < 15)
            {
                charsWritten = 0;
                return false;
            }
            fixed (char* reference = MemoryMarshal.GetReference<char>(formatted))
            {
                char* addressString = reference;
                charsWritten = IPAddressParser.IPv4AddressToStringHelper(address, addressString);
            }
            return true;
        }

        // Token: 0x06000126 RID: 294 RVA: 0x0000A8D4 File Offset: 0x000092D4
        private unsafe static int IPv4AddressToStringHelper(uint address, char* addressString)
        {
            int result = 0;
            IPAddressParser.FormatIPv4AddressNumber((int)(address & 255u), addressString, ref result);
            addressString[result++] = '.';
            IPAddressParser.FormatIPv4AddressNumber((int)(address >> 8 & 255u), addressString, ref result);
            addressString[result++] = '.';
            IPAddressParser.FormatIPv4AddressNumber((int)(address >> 16 & 255u), addressString, ref result);
            addressString[result++] = '.';
            IPAddressParser.FormatIPv4AddressNumber((int)(address >> 24 & 255u), addressString, ref result);
            return result;
        }

        // Token: 0x06000127 RID: 295 RVA: 0x0000A950 File Offset: 0x00009350
        internal static string IPv6AddressToString(ushort[] address, uint scopeId)
        {
            StringBuilder sb = IPAddressParser.IPv6AddressToStringHelper(address, scopeId);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        // Token: 0x06000128 RID: 296 RVA: 0x0000A96C File Offset: 0x0000936C
        internal static bool IPv6AddressToString(ushort[] address, uint scopeId, Span<char> destination, out int charsWritten)
        {
            StringBuilder stringBuilder = IPAddressParser.IPv6AddressToStringHelper(address, scopeId);
            if (destination.Length < stringBuilder.Length)
            {
                StringBuilderCache.Release(stringBuilder);
                charsWritten = 0;
                return false;
            }
            stringBuilder.CopyTo(0, destination, stringBuilder.Length);
            charsWritten = stringBuilder.Length;
            StringBuilderCache.Release(stringBuilder);
            return true;
        }

        // Token: 0x06000129 RID: 297 RVA: 0x0000A9B8 File Offset: 0x000093B8
        internal static StringBuilder IPv6AddressToStringHelper(ushort[] address, uint scopeId)
        {
            StringBuilder stringBuilder = StringBuilderCache.Acquire(65);
            if (IPv6AddressHelper.ShouldHaveIpv4Embedded(address))
            {
                IPAddressParser.AppendSections(address, 0, 6, stringBuilder);
                if (stringBuilder[stringBuilder.Length - 1] != ':')
                {
                    stringBuilder.Append(':');
                }
                IPAddressParser.IPv4AddressToString(IPAddressParser.ExtractIPv4Address(address), stringBuilder);
            }
            else
            {
                IPAddressParser.AppendSections(address, 0, 8, stringBuilder);
            }
            if (scopeId != 0u)
            {
                stringBuilder.Append('%').Append(scopeId);
            }
            return stringBuilder;
        }

        // Token: 0x0600012A RID: 298 RVA: 0x0000AA28 File Offset: 0x00009428
        private unsafe static void FormatIPv4AddressNumber(int number, char* addressString, ref int offset)
        {
            offset += ((number > 99) ? 3 : ((number > 9) ? 2 : 1));
            int num = offset;
            do
            {
                int num2;
                number = Math.DivRem(number, 10, out num2);
                addressString[--num] = (char)(48 + num2);
            }
            while (number != 0);
        }

        // Token: 0x0600012B RID: 299 RVA: 0x0000AA70 File Offset: 0x00009470
        public unsafe static bool Ipv4StringToAddress(ReadOnlySpan<char> ipSpan, out long address)
        {
            int length = ipSpan.Length;
            long num;
            fixed (char* reference = MemoryMarshal.GetReference<char>(ipSpan))
            {
                char* name = reference;
                num = IPv4AddressHelper.ParseNonCanonical(name, 0, ref length, true);
            }
            if (num != -1L && length == ipSpan.Length)
            {
                address = (long)(((ulong)-16777216 & (ulong)num) >> 24 | (ulong)((16711680L & num) >> 8) | (ulong)((ulong)(65280L & num) << 8) | (ulong)((ulong)(255L & num) << 24));
                return true;
            }
            address = 0L;
            return false;
        }

        // Token: 0x0600012C RID: 300 RVA: 0x0000AAE4 File Offset: 0x000094E4
        public unsafe static bool Ipv6StringToAddress(ReadOnlySpan<char> ipSpan, ushort* numbers, int numbersLength, out uint scope)
        {
            int length = ipSpan.Length;
            bool flag;
            fixed (char* reference = MemoryMarshal.GetReference(ipSpan))
            {
                char* name = reference;
                flag = IPv6AddressHelper.IsValidStrict(name, 0, ref length);
            }
            if (flag || length != ipSpan.Length)
            {
                string text = null;
                IPv6AddressHelper.Parse(ipSpan, numbers, 0, ref text);
                long num = 0L;
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Length < 2)
                    {
                        scope = 0u;
                        return false;
                    }
                    for (int i = 1; i < text.Length; i++)
                    {
                        char c = text[i];
                        if (c < '0' || c > '9')
                        {
                            scope = 0u;
                            return false;
                        }
                        num = num * 10L + (long)(c - '0');
                        if (num > (long)((ulong)-1))
                        {
                            scope = 0u;
                            return false;
                        }
                    }
                }
                scope = (uint)num;
                return true;
            }
            scope = 0u;
            return false;
        }

        // Token: 0x0600012D RID: 301 RVA: 0x0000ABA0 File Offset: 0x000095A0
        private static void AppendSections(ushort[] address, int fromInclusive, int toExclusive, StringBuilder buffer)
        {
            ReadOnlySpan<ushort> numbers = new ReadOnlySpan<ushort>(address, fromInclusive, toExclusive - fromInclusive);
            ValueTuple<int, int> valueTuple = IPv6AddressHelper.FindCompressionRange(numbers);
            int item = valueTuple.Item1;
            int item2 = valueTuple.Item2;
            bool flag = false;
            for (int i = fromInclusive; i < item; i++)
            {
                if (flag)
                {
                    buffer.Append(':');
                }
                flag = true;
                IPAddressParser.AppendHex(address[i], buffer);
            }
            if (item >= 0)
            {
                buffer.Append("::");
                flag = false;
                fromInclusive = item2;
            }
            for (int j = fromInclusive; j < toExclusive; j++)
            {
                if (flag)
                {
                    buffer.Append(':');
                }
                flag = true;
                IPAddressParser.AppendHex(address[j], buffer);
            }
        }

        // Token: 0x0600012E RID: 302 RVA: 0x0000AC34 File Offset: 0x00009634
        private unsafe static void AppendHex(ushort value, StringBuilder buffer)
        {
            char* ptr = stackalloc char[(UIntPtr)8];
            int num = 4;
            do
            {
                int num2 = (int)(value % 16);
                value /= 16;
                ptr[(IntPtr)(--num) * 2] = ((num2 < 10) ? ((char)(48 + num2)) : ((char)(97 + (num2 - 10))));
            }
            while (value != 0);
            buffer.Append(ptr + num, 4 - num);
        }

        // Token: 0x0600012F RID: 303 RVA: 0x0000AC86 File Offset: 0x00009686
        private static uint ExtractIPv4Address(ushort[] address)
        {
            return (uint)((int)IPAddressParser.Reverse(address[7]) << 16 | (int)IPAddressParser.Reverse(address[6]));
        }

        // Token: 0x06000130 RID: 304 RVA: 0x0000AC9C File Offset: 0x0000969C
        private static ushort Reverse(ushort number)
        {
            return (ushort)((number >> 8 & 255) | ((int)number << 8 & 65280));
        }
    }
}
}
