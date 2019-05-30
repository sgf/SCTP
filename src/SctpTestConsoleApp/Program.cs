using System;

namespace SctpTestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new SCTP();
            s.Run();
            Console.WriteLine("Hello World!");
            Console.ReadLine();



        }
    }
}
