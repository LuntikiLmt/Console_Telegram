using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    class Program
    {
        static void PrintDialogs(IServiceTL client)
        {
            int index = 0;
            foreach (var item in client.DialogsService.DialogList)
            {
                Console.WriteLine(index + " " + item.DialogName);
                index++;
            }
        }

        static async Task PrintDialogHistory(IServiceTL client)
        {
            int index;
            try
            {
                Console.Write("Input number of a dialog: ");
                int.TryParse(Console.ReadLine(), out index);
                var dlg = client.DialogsService.DialogList.ToList()[index];
                await client.DialogsService.FillDialog(dlg.DialogName, dlg.Peer, dlg.Id);
                Console.Clear();
                Console.WriteLine(client.DialogsService.Dialog.DialogName);
                foreach (var item in client.DialogsService.Dialog.Messages)
                {
                    Console.WriteLine(item.MessageDate + " from " + item.UserFirstName + " " + item.UserLastName + ": " + item.MessageText);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Number is incorrect! ");
                Console.WriteLine(e.ToString());
            }
        }



        static async Task Start()
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
            //var index = client.Fill();
            await client.FillAsync();

            PrintDialogs(client);

            await PrintDialogHistory(client);

            //var dialogs = client.DialogsService.DialogList.ToList();

            //Console.Write("Your text: ");
            //var text = Console.ReadLine();
            ////client.SendingService = ioc.Resolve<ISendingService>();
            ////client.SendingService.SendTextMessage(dialogs[index].Peer, dialogs[index].Id, text);
            //await client.SendingService.SendTextMessage(dialogs[0].Peer, dialogs[0].Id, text);
            //Console.WriteLine(text);

            //Console.WriteLine("Press any key...");
            //Console.Read();
        }

        static void Main(string[] args)
        {
            Start().Wait();

            Console.ReadKey();
        }
    }
}
