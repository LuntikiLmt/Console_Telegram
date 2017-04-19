using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;

namespace TeleWithVictorApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioC = new SimpleIoC();
            ioC.Register<IServiceTL, ServiceClient>();

            var client = ioC.Resolve<IServiceTL>();

            Console.ReadKey();
        }
    }
}
