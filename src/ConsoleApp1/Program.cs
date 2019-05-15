using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

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

            //var num = Convert.ToInt32("23", 16);
            //var sss = "Hello World!".AsSpan(0,20);

            //string s1 = "　的 ".Trim();
            

            var mx = new Mutex(true, "11111");
            Console.WriteLine("正在等待 the mutex");
            //申请
            if (mx.WaitOne(50, true)) Console.WriteLine("申请到 the mutex");
            else Console.WriteLine("未申请到 the mutex");
            Console.ReadLine();
            mx.Close();

            //Console.WriteLine(s1.Length);
            Console.WriteLine(sizeof(AAA));

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        [StructLayout(LayoutKind.Explicit, Pack = 2)]
        ref struct AAA
        {
            const int AA = 132;
            public int A2 { get { return AA; } }


            [FieldOffset(0)]
            public int A3;
        }

    }
}
