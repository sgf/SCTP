using SCTP;
using System;
using System.Buffers;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {


            var memory = MemoryPool<byte>.Shared.Rent(30).Memory;
            //memory.Write((ushort)12345);
            var buffer = memory.ToArray();
            NetworkHelpers.CopyTo((ushort)12345, buffer, 0);

            Console.WriteLine(buffer);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
