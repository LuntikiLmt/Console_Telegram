using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

        public event UpdateHandler OnUpdateDialogs;
        public event UpdateHandler OnUpdateContacts;

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
                    //Console.WriteLine("UpdateShort: "+ updateShort.Update);
                    //Console.WriteLine(updateShort.Update);
                    break;
                case TlUpdates updates:
                    //Console.WriteLine("Updates: "+updates.Updates);
                    //удалили диалог, нужно обновить диалоги
                    SystemSounds.Beep.Play();
                    if (updates.Updates.Lists.Count(item => item.GetType() == typeof(TlUpdateDeleteMessages)) != 0)
                        OnUpdateDialogs();
                    if (updates.Updates.Lists.Count(item => item.GetType() == typeof(TlUpdateContactLink)) != 0)
                        OnUpdateContacts();
                    break;
                //case TelegramClient.Entities.TlVector vector:
                //    break;
                default:
                    //Console.WriteLine("Default: "+update);
                    SystemSounds.Hand.Play();
                    break;
            }
        }

        public async Task Receieve()
        {
            Console.WriteLine("New massage!");
        }

    }
}
