using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{


    internal static class IPv4AddressHelper
    {
        // Token: 0x0600000B RID: 11 RVA: 0x00005734 File Offset: 0x00004134
        internal unsafe static int ParseHostNumber(ReadOnlySpan<char> str, int start, int end)
        {
            byte* ptr = stackalloc byte[4];
            IPv4AddressHelper.ParseCanonical(str, ptr, start, end);
            return ((*ptr) << 24) + (ptr[1] << 16) + (ptr[2] << 8) + ptr[3];
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00005769 File Offset: 0x00004169
        internal unsafe static bool IsValid(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile, bool unknownScheme)
        {
            if (allowIPv6 || unknownScheme)
            {
                return IPv4AddressHelper.IsValidCanonical(name, start, ref end, allowIPv6, notImplicitFile);
            }
            return IPv4AddressHelper.ParseNonCanonical(name, start, ref end, notImplicitFile) != -1L;
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00005790 File Offset: 0x00004190
        private unsafe static bool ParseCanonical(ReadOnlySpan<char> name, byte* numbers, int start, int end)
        {
            for (int i = 0; i < 4; i++)
            {
                byte b = 0;
                char c;
                while (start < end && (c = (name[start])) != '.' && c != ':')
                {
                    b = (byte)(b * 10 + (c - '0'));
                    start++;

                }
                numbers[i] = b;
                start++;
            }
            return *numbers == 127;
        }

        // Token: 0x0600000E RID: 14 RVA: 0x000057E8 File Offset: 0x000041E8
        internal unsafe static bool IsValidCanonical(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile)
        {
            int num = 0;
            int num2 = 0;
            bool flag = false;
            bool flag2 = false;
            while (start < end)
            {
                char c = name[start];
                if (allowIPv6)
                {
                    if (c == ']' || c == '/')
                    {
                        break;
                    }
                    if (c == '%')
                    {
                        break;
                    }
                }
                else if (c == '/' || c == '\\' || (notImplicitFile && (c == ':' || c == '?' || c == '#')))
                {
                    break;
                }
                if (c <= '9' && c >= '0')
                {
                    if (!flag && c == '0')
                    {
                        if (start + 1 < end && name[start + 1] == '0')
                        {
                            return false;
                        }
                        flag2 = true;
                    }
                    flag = true;
                    num2 = num2 * 10 + (int)(name[start] - '0');
                    if (num2 > 255)
                    {
                        return false;
                    }
                }
                else
                {
                    if (c != '.')
                    {
                        return false;
                    }
                    if (!flag || (num2 > 0 && flag2))
                    {
                        return false;
                    }
                    num++;
                    flag = false;
                    num2 = 0;
                    flag2 = false;
                }
                start++;
            }
            bool flag3 = num == 3 && flag;
            if (flag3)
            {
                end = start;
            }
            return flag3;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x000058E0 File Offset: 0x000042E0
        internal unsafe static long ParseNonCanonical(char* name, int start, ref int end, bool notImplicitFile)
        {
            long* ptr = stackalloc long[(UIntPtr)32];
            long num = 0L;
            bool flag = false;
            int num2 = 0;
            int i;
            for (i = start; i < end; i++)
            {
                char c = name[i];
                num = 0L;
                int num3 = 10;
                if (c == '0')
                {
                    num3 = 8;
                    i++;
                    flag = true;
                    if (i < end)
                    {
                        c = name[i];
                        if (c == 'x' || c == 'X')
                        {
                            num3 = 16;
                            i++;
                            flag = false;
                        }
                    }
                }
                while (i < end)
                {
                    c = name[i];
                    int num4;
                    if ((num3 == 10 || num3 == 16) && '0' <= c && c <= '9')
                    {
                        num4 = (int)(c - '0');
                    }
                    else if (num3 == 8 && '0' <= c && c <= '7')
                    {
                        num4 = (int)(c - '0');
                    }
                    else if (num3 == 16 && 'a' <= c && c <= 'f')
                    {
                        num4 = (int)(c + '\n' - 'a');
                    }
                    else
                    {
                        if (num3 != 16 || 'A' > c || c > 'F')
                        {
                            break;
                        }
                        num4 = (int)(c + '\n' - 'A');
                    }
                    num = num * (long)num3 + (long)num4;
                    if (num > (long)((ulong)-1))
                    {
                        return -1L;
                    }
                    flag = true;
                    i++;
                }
                if (i >= end || name[i] != '.')
                {
                    break;
                }
                if (num2 >= 3 || !flag || num > 255L)
                {
                    return -1L;
                }
                ptr[num2] = num;
                num2++;
                flag = false;
            }
            if (!flag)
            {
                return -1L;
            }
            if (i < end)
            {
                char c;
                if ((c = name[i]) != '/' && c != '\\' && (!notImplicitFile || (c != ':' && c != '?' && c != '#')))
                {
                    return -1L;
                }
                end = i;
            }
            ptr[num2] = num;
            switch (num2)
            {
                case 0:
                    if (*ptr > (long)((ulong)-1))
                    {
                        return -1L;
                    }
                    return *ptr;
                case 1:
                    if (ptr[1] > 16777215L)
                    {
                        return -1L;
                    }
                    return *ptr << 24 | (ptr[1] & 16777215L);
                case 2:
                    if (ptr[2] > 65535L)
                    {
                        return -1L;
                    }
                    return *ptr << 24 | (ptr[1] & 255L) << 16 | (ptr[2] & 65535L);
                case 3:
                    if (ptr[3] > 255L)
                    {
                        return -1L;
                    }
                    return *ptr << 24 | (ptr[1] & 255L) << 16 | (ptr[2] & 255L) << 8 | (ptr[3] & 255L);
                default:
                    return -1L;
            }
        }
    }

internal partial struct IPv4
    {
        internal unsafe static IPv4? Parse(ReadOnlySpan<char> ipSpan)
        {
            long address = 0;

            int length = ipSpan.Length;
            long num;
            fixed (char* reference = &MemoryMarshal.GetReference(ipSpan))// & ipSpan.GetPinnableReference()) //MemoryMarshal.GetReference(ipSpan)
            {
                char* name = reference;
                num = IPv4AddressHelper.ParseNonCanonical(name, 0, ref length, true);
            }
//#nullable disable
//#nullable restore
            if (num != -1L && length == ipSpan.Length)
            {
                address = (long)(((ulong)-16777216 & (ulong)num) >> 24 | (ulong)((16711680L & num) >> 8) | (ulong)((ulong)(65280L & num) << 8) | (ulong)((ulong)(255L & num) << 24));
                return new IPv4 { Octets = address };
                return true;
            }
            address = 0L;
            return false;


            uint newAddress;
            if (IPAddressParser.Ipv4StringToAddress(ipSpan, out newAddress))
            {
            }
            throw new FormatException("字符串并非IPv4格式", new SocketException((int)SocketError.InvalidArgument));
        }




    }



    internal partial struct IPv6
    {

        internal unsafe static IPv6? Parse(ReadOnlySpan<char> ipSpan)
        {

            if (ipSpan.Contains(':'))
            {
                ushort* ptr = stackalloc ushort[16];
                new Span<ushort>((void*)ptr, 8).Clear();
                uint scopeid;
                if (IPAddressParser.Ipv6StringToAddress(ipSpan, ptr, 8, out scopeid))
                {
                    return new IPv6(ptr, 8, scopeid);
                }
            }
            throw new FormatException("字符串并非IPv6格式", new SocketException((int)SocketError.InvalidArgument));
        }
    }

    internal class IPAddressParser
    {
        // Token: 0x06000122 RID: 290 RVA: 0x0000A7EC File Offset: 0x000091EC
        internal unsafe static IPAddress Parse(ReadOnlySpan<char> ipSpan, bool tryParse)
        {
            long newAddress;
            if (ipSpan.Contains(':'))
            {
                ushort* ptr = stackalloc ushort[16];
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
            fixed (char* reference = &MemoryMarshal.GetReference(ipSpan))
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
