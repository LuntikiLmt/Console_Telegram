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
            _client.Updates.RecieveUpdates += Updates_RecieveUpdates;
        }

        private void Updates_RecieveUpdates(TlAbsUpdates update)
        {
            switch (update)
            {
                case TlUpdateShort updateShort:
                    Console.WriteLine("UpdateShort: "+ updateShort.Update);
                    //Console.WriteLine(updateShort.Update);
                    break;
                case TlUpdates updates:
                    Console.WriteLine("Updates: "+updates.Updates);
                    break;
                //case TelegramClient.Entities.TlVector vector:
                //    break;
                default:
                    Console.WriteLine("Default: "+update);
                    break;
            }
        }

        public async Task Receieve()
        {
            Console.WriteLine("New massage!");
        }

    }
}
