using System;
using System.Runtime.CompilerServices;

namespace XDivert
{

    public class Macro
    {


        //#define WINDIVERT_IPHDR_GET_FRAGOFF(hdr)                    \
        //        (((hdr)->FragOff0) & 0xFF1F)m>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPHDR_GET_FRAGOFF(dynamic hdr) => (hdr.FragOff0) & 0xFF1F;
        //#define WINDIVERT_IPHDR_GET_MF(hdr)                         \
        //        ((((hdr)->FragOff0) & 0x0020) != 0)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPHDR_GET_MF(dynamic hdr) => (hdr.FragOff0 & 0x0020) != 0;
        //#define WINDIVERT_IPHDR_GET_DF(hdr)                         \
        //        ((((hdr)->FragOff0) & 0x0040) != 0)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPHDR_GET_DF(dynamic hdr) => (hdr.FragOff0 & 0x0040) != 0;
        //#define WINDIVERT_IPHDR_GET_RESERVED(hdr)                   \
        //        ((((hdr)->FragOff0) & 0x0080) != 0)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPHDR_GET_RESERVED(dynamic hdr) => (hdr.FragOff0 & 0x0080) != 0;
        //      #define WINDIVERT_IPHDR_SET_FRAGOFF(hdr, val)               \
        //    do                                                      \
        //    {                                                       \
        //        (hdr)->FragOff0 = (((hdr)->FragOff0) & 0x00E0) |    \
        //            ((val) & 0xFF1F);                               \
        //    }                                                       \
        //    while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WINDIVERT_IPHDR_SET_FRAGOFF(dynamic hdr, dynamic val) => hdr.FragOff0 = (hdr.FragOff0 & 0x00E0) | ((val) & 0xFF1F);
        //    #define WINDIVERT_IPHDR_SET_MF(hdr, val)                    \
        //  do                                                      \
        //  {                                                       \
        //      (hdr)->FragOff0 = (((hdr)->FragOff0) & 0xFFDF) |    \
        //          (((val) & 0x0001) << 5);                        \
        //  }                                                       \
        //  while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WINDIVERT_IPHDR_SET_MF(dynamic hdr, dynamic val) => hdr.FragOff0 = (hdr.FragOff0 & 0xFFDF) | (((val) & 0x0001) << 5);

        //    #define WINDIVERT_IPHDR_SET_DF(hdr, val)                    \
        //  do                                                      \
        //  {                                                       \
        //      (hdr)->FragOff0 = (((hdr)->FragOff0) & 0xFFBF) |    \
        //          (((val) & 0x0001) << 6);                        \
        //  }                                                       \
        //  while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WINDIVERT_IPHDR_SET_DF(dynamic hdr, dynamic val) => hdr.FragOff0 = (hdr.FragOff0 & 0xFFBF) | ((val & 0x0001) << 6);

        //  #define WINDIVERT_IPHDR_SET_RESERVED(hdr, val)              \
        //    do                                                      \
        //    {                                                       \
        //        (hdr)->FragOff0 = (((hdr)->FragOff0) & 0xFF7F) |    \
        //            (((val) & 0x0001) << 7);                        \
        //    }                                                       \
        //    while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static void WINDIVERT_IPHDR_SET_RESERVED(dynamic hdr, dynamic val) => hdr.FragOff0 = (hdr.FragOff0 & 0xFF7F) | ((val & 0x0001) << 7);

        //  #define WINDIVERT_IPV6HDR_GET_TRAFFICCLASS(hdr)             \
        //        ((((hdr)->TrafficClass0) << 4) | ((hdr)->TrafficClass1))
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPV6HDR_GET_TRAFFICCLASS(dynamic hdr) => ((hdr.TrafficClass0) << 4) | (hdr.TrafficClass1);

        //#define WINDIVERT_IPV6HDR_GET_FLOWLABEL(hdr)                \
        //        ((((UINT32)(hdr)->FlowLabel0) << 16) | ((UINT32)(hdr)->FlowLabel1))
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic WINDIVERT_IPV6HDR_GET_FLOWLABEL(dynamic hdr) => (((uint)hdr.FlowLabel0) << 16) | ((uint)hdr.FlowLabel1);
        //    #define WINDIVERT_IPV6HDR_SET_TRAFFICCLASS(hdr, val)        \
        //  do                                                      \
        //  {                                                       \
        //      (hdr)->TrafficClass0 = ((UINT8)(val) >> 4);         \
        //      (hdr)->TrafficClass1 = (UINT8) (val);                \
        //  }                                                       \
        //  while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WINDIVERT_IPV6HDR_SET_TRAFFICCLASS(dynamic hdr, dynamic val)
        {
            hdr.TrafficClass0 = ((byte)(val) >> 4);
            hdr.TrafficClass1 = (byte)(val);
        }

        //#define WINDIVERT_IPV6HDR_SET_FLOWLABEL(hdr, val)           \
        //    do                                                      \
        //    {                                                       \
        //        (hdr)->FlowLabel0 = (UINT8) ((val) >> 16);           \
        //        (hdr)->FlowLabel1 = (UINT16) (val);                  \
        //    }                                                       \
        //    while (FALSE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WINDIVERT_IPV6HDR_SET_FLOWLABEL(dynamic hdr, dynamic val)
        {
            hdr.FlowLabel0 = (byte)((val) >> 16);
            hdr.FlowLabel1 = (ushort)(val);
        }

    }
















}
