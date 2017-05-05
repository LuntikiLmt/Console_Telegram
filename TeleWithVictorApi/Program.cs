using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

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

        static void PrintContacts(IServiceTL client)
        {
            int index = 0;
            foreach (var item in client.ContactsService.Contacts)
            {
                Console.WriteLine(index + " " + item.FirstName + " " + item.LastName);
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

        static async Task SendMessageToDialogByNumber(IServiceTL client, int index)
        {
            var dialogs = client.DialogsService.DialogList.ToList();
            Console.Write("Your text: ");
            var text = Console.ReadLine();
            await client.SendingService.SendTextMessage(dialogs[index].Peer, dialogs[index].Id, text);
            Console.WriteLine(text);
        }

        static async Task SendMessageToContactByNumber(IServiceTL client, int index)
        {
            var contacts = client.ContactsService.Contacts.ToList();
            Console.Write("Your text: ");
            var text = Console.ReadLine();
            await client.SendingService.SendTextMessage(Peer.User, contacts[index].Id, text);
            Console.WriteLine(text);
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
            await client.FillAsync();

            var dialogs = client.DialogsService.DialogList.ToList();
            for (int i = 0; i < dialogs.Count; i++)
            {
                Console.WriteLine($"{i} {dialogs[i].DialogName}");
            }
                
            string invVerb = "";
            object invSubop = null;

            //after registration enter "send -t 'dialog_number' -m 'text_message'"
            var options = new Options();
            var line = Console.ReadLine().Split(' ');

            var result = Parser.Default.ParseArguments(line, options,
                (verb, subOp) =>
                {
                    invVerb = verb;
                    invSubop = subOp;
                });

            int index;
            string message;

            if (result)
            {
                if (invVerb == "send")
                {
                    message = (invSubop as SendOptions).Message;
                    Int32.TryParse((invSubop as SendOptions).Target, out index);

                    await SendMessage(client, index, message);
                }
            }
        }

        static void Main(string[] args)
        {
            Start().Wait();
  

            Console.ReadKey();
        }

        static async Task SendMessage(IServiceTL client, int index, string text)
        {
            var dialogs = client.DialogsService.DialogList.ToList();
            await client.SendingService.SendTextMessage(dialogs[index].Peer, dialogs[index].Id, text);
            Console.WriteLine(text);
        }
    }
}
