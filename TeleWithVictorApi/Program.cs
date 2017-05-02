using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioc = new SimpleIoC();
            #region RegisterIoC
            ioc.RegisterInstance(TelegramClient.Core.ClientFactory.BuildClient(35699, "c5faabe85e286bbb3eac32df78b34517", "149.154.167.50", 443));
            ioc.Register<IContactsService, ContactsService>();
            ioc.Register<IDialogsService, DialogsService>();
            ioc.Register<ISendingService, SendingService>();

            ioc.Register<IServiceTL, ServiceClient>();

            ioc.Register<IContact, Contact>();
            ioc.Register<IDialog, Dialog>();
            ioc.Register<IMessage, Message>();
            ioc.Register<IDialogShort, DialogShort>();
            #endregion
            var client = ioc.Resolve<IServiceTL>();
            var index = client.Fill();

            var dialogs = client.DialogsService.DialogList.ToList();
            
            Console.Write("Your text: ");
            var text = Console.ReadLine();
            client.SendingService = ioc.Resolve<ISendingService>();
            client.SendingService.SendTextMessage(dialogs[index].Peer, dialogs[index].Id, text);
            Console.WriteLine(text);

            Console.WriteLine("Press any key...");
            Console.Read();
        }
    }
}
