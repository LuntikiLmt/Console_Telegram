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
            //if (update is TlUpdates)
            //{
            //    var list = (update as TlUpdates).Updates.Lists;
            //    foreach (var tlAbsUpdate in list)
            //    {
            //        if (tlAbsUpdate is TlUpdateNewMessage)
            //        {
            //            var message = (tlAbsUpdate as TlUpdateNewMessage).Message as TlMessage;
            //            string text = message?.Message;
            //            string sender = String.Empty;
            //            int? from = message?.FromId;
            //            foreach (var item in ContactsService.Contacts)
            //            {
            //                if (from == item.Id)
            //                {
            //                    sender = item.FirstName + " " + item.LastName;
            //                }
            //            }
            //            //foreach (var dialogShort in DialogsService.DialogList)
            //            //{
            //            //    if (from == dialogShort.Id)
            //            //    {
            //            //        sender = dialogShort.DialogName;
            //            //    }
            //            //}

            //            Console.WriteLine("Message: \"" + text + "\" send from " + sender);
            //        }
            //        if (tlAbsUpdate is TlUpdateDraftMessage)
            //        {
            //            string message = ((tlAbsUpdate as TlUpdateDraftMessage).Draft as TlDraftMessage)?.Message;
            //            Console.WriteLine(message);
            //        }
            //    }
            //}
        }

        public async Task Receieve()
        {
            Console.WriteLine("New massage!");
        }

    }
}
