using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace XSCTP
{




    [StructLayout(LayoutKind.Explicit, Pack = 1)]
   internal unsafe partial struct IPv4 : IComparable
    {
        [FieldOffset(0)]
        public uint Octets;
        [FieldOffset(0)]
        public byte _0;
        [FieldOffset(1)]
        public byte _1;
        [FieldOffset(2)]
        public byte _2;
        [FieldOffset(3)]
        public byte _3;

        public override string ToString()
        {
            return $"{_0}.{_1}.{_2}.{_3}";
        }

        public int CompareTo(object other)
        {
            if (other != null &&
                other is IPv4 ipv4)
            {
                return (this.Octets > ipv4.Octets ? 1 : (this.Octets == ipv4.Octets ? 0 : -1));
            }
            return 1;
        }

        public static bool IsIPv4Address(string address)
        {
            if (address == null || address.Length == 0) return false;
            var arr = address.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 4) return false;
            return arr.IsArrayItemWithinRange(0, 255, false);
        }
        public IPv4 Parse(uint src) {
          return  new IPv4 { Octets = src };
        }
        public IPv4? Parse(byte[] src)
        {
            return new IPv4 { Octets = BitConverter.ToUInt32(src, 0) };
        }
        public IPv4? Parse(string ip)
        {
            IPAddress.Parse(ip).
        }
        public IPv4? Parse()
        {

        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal unsafe struct IPv6 : IComparable
    {
        [FieldOffset(0)]
        public Int128 Value;
        [FieldOffset(0)]
        public fixed byte Octets[16];
        [FieldOffset(0)]
        public ushort _0;
        [FieldOffset(1)]
        public ushort _1;
        [FieldOffset(2)]
        public ushort _2;
        [FieldOffset(3)]
        public ushort _3;
        [FieldOffset(4)]
        public ushort _4;
        [FieldOffset(5)]
        public ushort _5;
        [FieldOffset(6)]
        public ushort _6;
        [FieldOffset(7)]
        public ushort _7;




        public ushort Length => 16;

        public override string ToString()
        {
            return $"{_0:X}:{_1:X}:{_2:X}:{_3:X}:{_4:X}:{_5:X}:{_6:X}:{_7:X}:";
        }

        //public (bool ok, ref IPv6Address ipv6) TryPrase(string str) {

        //}

        public unsafe int CompareTo(object other)
        {
            if (other != null &&
                other is IPv6 ipv6)
            {
                var _ipv6 = new BigInteger(new ReadOnlySpan<byte>(ipv6.Octets, 16));
                fixed (void* b = Octets)
                {
                    var _this = new BigInteger(new ReadOnlySpan<byte>(b, 16));
                    return (_this > _ipv6 ? 1 : (_this == _ipv6 ? 0 : -1));
                }
            }
            return 1;
        }

        public static bool IsIPv6Address(string address)
        {
            if (address == null || address.Length == 0) return false;

            var arr = address.Split("::", StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 2) return false;
            //双冒号法 格式举例 2000::1:2345:6789:abcd
            //格式举例 2000:0000:0000:0000:0001:2345:6789:abcd
            //前导零压缩法 2000:0:0:0:1:2345:6789:abcd

            if (arr.Length == 2)
            {
                string[] arr1 = null;
                string[] arr2 = null;

                if (arr[0] != "") arr1 = arr[0].Split(":");

                if (arr[1] != "") arr2 = arr[1].Split(":");

                if (arr1 != null && arr2 != null && arr1.Length + arr2.Length > 6) return false;

                if (arr1 != null)
                {
                    if (arr1.Length > 6) return false;
                    if (!arr1.IsArrayItemWithinRange(0, 65535, true)) return false;
                }

                if (arr2 != null)
                {
                    if (arr2.Length > 6) return false;
                    if (!arr2.IsArrayItemWithinRange(0, 65535, true)) return false;
                }
            }
            else
            {
                arr = address.Split(":");
                if (arr.Length != 8) return false;
                if (!arr.IsArrayItemWithinRange(0, 65535, true)) return false;
            }

            return true;
        }
    }

    static class IP4AddressEx
    {
        static int IPv4ToInt(this IPv4 address)
        {
            return (address._3 << 24) +
                (address._2 << 16) +
                (address._1 << 8) +
                (address._0);
        }
    }

    public static class StringEx
    {
        public static int? ToInt(this string src, bool hexDigit)
        {
            if (hexDigit)
                return int.TryParse("0x011", NumberStyles.AllowHexSpecifier | NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int _rltFrom16) ? _rltFrom16 : (int?)null;

            return int.TryParse(src, out int _rltFrom10) ? _rltFrom10 : (int?)null;
        }

        public static bool IsArrayItemWithinRange(this string[] arr, int start, int end, bool hexDigit)
        {
            foreach (var a in arr)
            {
                if (a.Length == 0) return false;
                var i = a.ToInt(hexDigit);

                if (i == null) return false;

                if (i < start || i > end) return false;
            }
            return true;
        }
    }

}
