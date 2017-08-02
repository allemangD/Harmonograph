using System;
using System.Threading;

namespace Harmonograph
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string f = null;
            if (args.Length > 0) f = args[0];
            if (f == "-")
            {
                Console.Write("input file.\n>>> ");
                f = Console.ReadLine() + ".json";
            }
            using (var w = new GraphWindow(f))
            {
                w.Run();
            }
        }
    }
}