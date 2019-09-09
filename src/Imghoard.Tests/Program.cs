using System;
using System.IO;
using System.Threading.Tasks;

namespace Imghoard.Tests
{
    class Program
    {
        static void Main()
            => RunAsync().GetAwaiter().GetResult();

        static async Task RunAsync()
        {
            var cli = new ImghoardClient();
            
            Console.ReadLine();
        }
    }
}
