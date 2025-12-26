using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "TCP Chat Client";

            Console.WriteLine("=== TCP Chat Client ===");
            await Client.RunAsync();
        }
    }
}
