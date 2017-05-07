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
        static void PrintDialogs(IServiceTl client)
        {
            int index = 0;
            Console.WriteLine("Dialogs:");
            foreach (var item in client.DialogsService.DialogList)
            {
                Console.WriteLine(index + " " + item.DialogName);
                index++;
            }
        }

        static void PrintContacts(IServiceTl client)
        {
            int index = 0;
            Console.WriteLine("Contacts:");
            foreach (var item in client.ContactsService.Contacts)
            {
                Console.WriteLine(index + " " + item.FirstName + " " + item.LastName);
                index++;
            }
        }

        static async Task PrintDialogHistory(IServiceTl client)
        {
            try
            {
                Console.Write("Input number of a dialog: ");
                int.TryParse(Console.ReadLine(), out int index);
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

        static async Task SendMessageToDialogByNumber(IServiceTl client, int index)
        {
            var dialogs = client.DialogsService.DialogList.ToList();
            Console.Write("Your text: ");
            var text = Console.ReadLine();
            await client.SendingService.SendTextMessage(dialogs[index].Peer, dialogs[index].Id, text);
            Console.WriteLine(text);
        }

        static async Task SendMessageToContactByNumber(IServiceTl client, int index)
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

            ioc.Register<IServiceTl, ServiceClient>();

            ioc.Register<IContact, Contact>();
            ioc.Register<IDialog, Dialog>();
            ioc.Register<IMessage, Message>();
            ioc.Register<IDialogShort, DialogShort>();
            #endregion
            var client = ioc.Resolve<IServiceTl>();
            await client.FillAsync();

            async void Send(SendOptions opt)
            {
                var builder = new StringBuilder();
                builder.Append(opt.Message.First());
                for (int i = 1; i < opt.Message.Count(); ++i)
                {
                    builder.Append(' ');
                    builder.Append(opt.Message.ElementAt(i));
                }
                var message = builder.ToString();
                if (opt.DialogIndex != -1)
                {
                    await SendMessageToDialog(client, opt.DialogIndex, message);
                }
                else if (opt.ContactIndex != -1)
                {
                    await SendMessageToContact(client, opt.ContactIndex, message);
                }
                else
                {
                    Console.WriteLine("Add '-d' to send to dialog or '-c' to send to contact");
                }
            }

            async void Print(PrintOptions opt)
            {
                if (opt.Contacts)
                {
                    PrintContacts(client);
                }
                if (opt.Dialogs)
                {
                    PrintDialogs(client);
                }
                if (opt.Index != -1)
                {
                    await PrintDialogMessages(client, opt.Index);
                }
            }


            bool isRun = true;

            while (isRun)
            {
                var line = Console.ReadLine()?.Split(' ');
                var parseResult = Parser.Default.ParseArguments<PrintOptions, SendOptions, AddContactOptions, Quit>(line);

                parseResult.
                    WithParsed<Quit>(quit => isRun = false).
                    WithParsed((Action<PrintOptions>) Print).
                    WithParsed((Action<SendOptions>) Send).
                    WithParsed<AddContactOptions>(async opt => await client.ContactsService.AddContact(opt.FirstName, opt.LastName, opt.Number));
            }
        }

        static void Main(string[] args)
        {
            Start().Wait();
        }

        static async Task SendMessageToDialog(IServiceTl client, int index, string text)
        {
            var dialogs = client.DialogsService.DialogList;
            await client.SendingService.SendTextMessage(dialogs.ElementAt(index).Peer, dialogs.ElementAt(index).Id, text);
            Console.WriteLine(text);
        }

        static async Task SendMessageToContact(IServiceTl client, int index, string text)
        {
            var contacts = client.ContactsService.Contacts;
            await client.SendingService.SendTextMessage(Peer.User, contacts.ElementAt(index).Id, text);
            Console.WriteLine(text);
        }

        static async Task PrintDialogMessages(IServiceTl client, int index)
        {
            try
            {
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
    }
}
