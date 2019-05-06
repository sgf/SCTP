using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {
        static unsafe void Main(string[] args)
        {

            //var memory = MemoryPool<byte>.Shared.Rent(30).Memory;
            ////memory.Write((ushort)12345);
            //var buffer = memory.ToArray();
            //NetworkHelpers.CopyTo((ushort)12345, buffer, 0);

            Convert.ToInt32("", 16);

           


            Console.WriteLine(num);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }


        struct AAA
        {
            const int AA = 132;
            public int A2 { get { return AA; } }

        }

    }
}
