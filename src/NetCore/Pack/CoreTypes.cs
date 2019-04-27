using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Pack
{
    pattern InitOptionalOrVariableParameter = IPv4AddressParameter | // 5
        IPv6AddressParameter | // 6
        CookiePreservative | // 9
        HostNameAddress | // 11
        SupportedAddressTypes | // 12
        EcnParameter;                                                // 32768

    class IPv4AddressParameter
    {
    ushort class where value == 5;
    ushort Length where value == 8;
        IPv4Address IPv4Address;
    }

    class IPv6AddressParameter
    {
    ushort class where value == 6;
    ushort Length where value == 20;
        IPv6Address IPv6Address;
    }

    class CookiePreservative
    {
    ushort class where value == 9;
    ushort Length where value == 8;
    uint SuggestedCookieLifeSpanIncrement;
    }

    class HostNameAddress
    {
    ushort class where value == 11;
    ushort Length;
    string HostName with BinaryEncoding { Length = Length - 4,TextEncoding = TextEncoding.ASCII};
    }


    class IPv6AddressParameter
    {
        ushort class == 6;
        ushort Length = 20;
        IPv6Address IPv6Address;
    }


}
