using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Updates;

namespace TeleWithVictorApi
{
    class Program
    {
        public static void sdfdf()
        {
            ConsoleKeyInfo cki;

            Console.WriteLine("Press any combination of CTL, ALT, and SHIFT, and a console key.");
            Console.WriteLine("Press the Escape (Esc) key to quit: \n");
            do
            {
                cki = Console.ReadKey();
                Console.Write(" --- You pressed ");
                if ((cki.Modifiers & ConsoleModifiers.Alt) != 0) Console.Write("ALT+");
                if ((cki.Modifiers & ConsoleModifiers.Shift) != 0) Console.Write("SHIFT+");
                if ((cki.Modifiers & ConsoleModifiers.Control) != 0) Console.Write("CTRL+");
                Console.WriteLine(cki.Key.ToString());
            } while (cki.Key != ConsoleKey.Escape);
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
            };

            var ioc = new SimpleIoC();

            #region RegisterIoC
            ioc.RegisterInstance(TelegramClient.Core.ClientFactory.BuildClient(35699, "c5faabe85e286bbb3eac32df78b34517", "149.154.167.40", 443));
            ioc.Register<IContactsService, ContactsService>();
            ioc.Register<IDialogsService, DialogsService>();
            ioc.Register<ISendingService, SendingService>();
            ioc.Register<IReceivingService, ReceivingService>();

            ioc.Register<IServiceTl, ServiceClient>();

            ioc.Register<IContact, Contact>();
            ioc.Register<IDialog, Dialog>();
            ioc.Register<IMessage, Message>();
            ioc.Register<IDialogShort, DialogShort>();
            #endregion

            var client = ioc.Resolve<IServiceTl>();

            //Start(client).Wait();
            Authorize(client);
            client.FillAsync().Wait();

            client.ReceivingService.OnAddUnreadMessageFromUser += (id, text, dateTime) =>
            {
                var message = ioc.Resolve<IMessage>();
                var user = client.ContactsService.Contacts.FirstOrDefault(c => c.Id == id);
                if (user?.ToString() == client.DialogsService.Dialog.DialogName)
                {
                    message.Fill(user?.ToString(), text, dateTime);
                    Console.WriteLine(message);
                }
            };

            client.ReceivingService.OnAddUnreadMessageFromChannel += (title, text, dateTime) =>
            {
                if (title == client.DialogsService.Dialog.DialogName)
                {
                    var message = ioc.Resolve<IMessage>();
                    message.Fill(title, text, dateTime);
                    Console.WriteLine(message);
                }
            };

            Action<SendOptions> Send = async opt =>
            {
                var builder = new StringBuilder();
                builder.Append(opt.Message.First());
                for (int i = 1; i < opt.Message.Count(); ++i)
                {
                    builder.Append(' ');
                    builder.Append(opt.Message.ElementAt(i));
                }
                var message = builder.ToString();
                try
                {
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
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("Number is incorrect!");
                }
            };

            Action<PrintOptions> Print = opt =>
            {
                if (opt.Contacts)
                {
                    PrintContacts(client);
                }
                if (opt.Dialogs)
                {
                    PrintDialogs(client);
                }
                if (opt.UnreadMessages)
                {
                    PrintUnreadMessages(client);
                }
                if (opt.Index != -1)
                {
                    try
                    {
                        PrintDialogMessages(client, opt.Index).Wait();
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Console.WriteLine("Number is incorrect!");
                    }
                    while (true)
                    {
                        string mes = Console.ReadLine();
                        if (mes == null)
                        {
                            break;
                        }
                        if (mes.Substring(0, 2) == "-f")
                        {
                            mes = mes.Remove(0, 2).TrimStart(' ');
                            SendMessageToDialog(client, opt.Index, mes, "file");
                        }
                        else
                        {
                            SendMessageToDialog(client, opt.Index, mes);
                        }
                    }
                }
            };

            bool isRun = true;
            while (isRun)
            {
                Console.Write("\n->");
                var line = Console.ReadLine()?.Split(' ');
                var parseResult = Parser.Default.ParseArguments<PrintOptions, SendOptions, SendFileOptions, AddContactOptions, DeleteContactOptions, Quit, LogOutOptions>(line);

                parseResult.
                    WithParsed<Quit>(quit => isRun = false).
                    WithParsed(Print).
                    WithParsed(Send).
                    WithParsed<DeleteContactOptions>(async opt => await client.ContactsService.DeleteContact(opt.Index)).
                    WithParsed<AddContactOptions>(async opt => await client.ContactsService.AddContact(opt.FirstName, opt.LastName, opt.Number)).
                    WithParsed<LogOutOptions>(logout => {
                        //client.LogOut();
                        Authorize(client);
                        client.FillAsync().Wait();
                    });
            }
        }

        static void Authorize(IServiceTl client)
        {
            if (!client.Authorize())
            {
                string phone, code;
                do
                {
                    Console.Write("Enter your phone number:\n+7");
                    phone = $"7{Console.ReadLine()}";
                }
                while (!Validation.PhoneValidation(phone));

                client.EnterPhoneNumber(phone);
                do
                {
                    Console.WriteLine("Enter incoming code:");
                    code = Console.ReadLine();
                }
                while (!client.EnterIncomingCode(code).Result);
                
            }
            Console.WriteLine("Welcome");
        }

        static async Task SendMessageToDialog(IServiceTl client, int index, string text)
        {
            var dialog = client.DialogsService.DialogList.ElementAt(index);
            await client.SendingService.SendTextMessage(dialog.Peer, dialog.Id, text);
            await client.DialogsService.FillDialog(dialog.DialogName, dialog.Peer, dialog.Id);
            int count = client.DialogsService.Dialog.Messages.Count();
            Console.WriteLine(client.DialogsService.Dialog.Messages.ElementAt(count - 1));
        }

        static async Task SendMessageToDialog(IServiceTl client, int index, string path, string caption)
        {
            var dialog = client.DialogsService.DialogList.ElementAt(index);
            await client.DialogsService.FillDialog(dialog.DialogName, dialog.Peer, dialog.Id);
            await client.SendingService.SendFile(dialog.Peer, dialog.Id, path, caption);
            Console.WriteLine("File otpravlen");
            //int count = client.DialogsService.Dialog.Messages.Count();
            //Console.WriteLine(client.DialogsService.Dialog.Messages.ElementAt(count - 1));
        }

        static async Task SendMessageToContact(IServiceTl client, int index, string text)
        {
            var contacts = client.ContactsService.Contacts;
            await client.SendingService.SendTextMessage(Peer.User, contacts.ElementAt(index).Id, text);
            Console.WriteLine(text);
            //может быть так, что создался новый диалог, поэтому нужно обновить список
            client.DialogsService.FillDialogList();
        }

        static async Task PrintDialogMessages(IServiceTl client, int index)
        {
            var dlg = client.DialogsService.DialogList.ToList()[index];
            await client.DialogsService.FillDialog(dlg.DialogName, dlg.Peer, dlg.Id);
            //Console.Clear();
            Console.WriteLine($"{client.DialogsService.Dialog.DialogName}");
            foreach (var item in client.DialogsService.Dialog.Messages)
            {
                Console.WriteLine(item);
            }
        }

        static void PrintDialogs(IServiceTl client)
        {
            int index = 0;
            Console.WriteLine("\nDialogs:");
            foreach (var item in client.DialogsService.DialogList)
            {
                Console.WriteLine($"{index} {item.DialogName}");
                index++;
            }
        }

        static void PrintContacts(IServiceTl client)
        {
            int index = 0;
            Console.WriteLine("\nContacts:");
            foreach (var item in client.ContactsService.Contacts)
            {
                Console.WriteLine($"{index} {item}");
                index++;
            }
        }

        static void PrintUnreadMessages(IServiceTl client)
        {
            int index = 0;
            if (client.ReceivingService.UnreadMessages.Count == 0)
            {
                Console.WriteLine("\nNo unread messages");
            }
            else
            {
                Console.WriteLine("\nUnread messages:");
                foreach (var item in client.ReceivingService.UnreadMessages)
                {
                    Console.WriteLine($"{index} {item}");
                    index++;
                }
                client.ReceivingService.UnreadMessages.Clear();
            }
        }
    }
}
