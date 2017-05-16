using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Updates;

namespace TeleWithVictorApi
{
    
    class ReceivingService : IReceivingService
    {
        private readonly ITelegramClient _client;
        
        public ReceivingService(SimpleIoC ioc)
        {
            _client = ioc.Resolve<ITelegramClient>();
        }
        

        public async Task Receieve()
        {
            Console.WriteLine("New massage!");
        }

    }
}
