using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Entities.TL.Messages;
using TelegramClient.Core;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    class ServiceClient : IServiceTL
    {
        private ITelegramClient _client;
        private SimpleIoC _ioc;

        public IContactsService ContactsService { get; set; }
        public IDialogsService DialogsService { get ; set; }
        public ISendingService SendingService { get; set; }
        public IReceivingService ReceivingService { get; set; }

        public ServiceClient(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = _ioc.Resolve<ITelegramClient>();

            Authenticate().Wait();
        }

        public void Fill()
        {
            DialogsService = _ioc.Resolve<IDialogsService>();
            //ContactsService = _ioc.Resolve<IContactsService>();

            //DialogsService.FillDialog(new TlPeerChannel(), "Лаборатория .Net 2017").Wait(); 
            //DialogsService.FillDialog(new TlPeerChat(), "Лунтики").Wait();
            DialogsService.FillDialog(new TlPeerUser(), "АртурИванов").Wait();
            Console.WriteLine(DialogsService.Dialog.DialogName); 
            foreach (var item in DialogsService.Dialog.Messages)
            {
                Console.WriteLine(item.MessageDate + " " + item.MessageText);
            }

            //ContactsService.FillContacts().Wait();
        }

        private bool Validate_InputPhone(string phone)
        {
            if (phone[0] == '7' && phone.Length == 11)
            {
                return true;
            }

            return false;
        }
        private async Task Authenticate()
        {
            await _client.ConnectAsync();
            if (!_client.IsUserAuthorized())
            {
                string phoneNumber = string.Empty;
                Console.WriteLine("Input your phone number please: ");
                bool isPhoneCorrect = false;
                while (!isPhoneCorrect)
                {
                    Console.WriteLine("Number should be in format (7##########):");
                    phoneNumber = Console.ReadLine();
                    isPhoneCorrect = Validate_InputPhone(phoneNumber);
                }

                Console.WriteLine("Input a code, send to you in telegram: ");

                string hash = string.Empty;
                try
                {
                    hash = await _client.SendCodeRequestAsync(phoneNumber);
                }
                catch (Exception e)
                {

                }

                bool isCodeCorrect = false;
                while (!isCodeCorrect)
                {
                    var code = Console.ReadLine();
                    try
                    {
                        await _client.MakeAuthAsync(phoneNumber, hash, code);

                        isCodeCorrect = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Code is incorrect! Try again please: ");
                        Console.WriteLine(e.ToString());
                    }
                }
                
            }
            Console.WriteLine("Welcome!");
        }
    }

    class ContactsService : IContactsService
    {
        private ITelegramClient _client;
        private SimpleIoC _ioc;

        public IEnumerable<IContact> Contacts { get; private set; }

        public ContactsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task FillContacts()
        {
            var cont = await _client.GetContactsAsync();
            IEnumerable<TlUser>users = cont.Users.Lists.Cast<TlUser>();
            List<IContact> contacts = new List<IContact>();
            Contacts = new List<IContact>();
            foreach (var item in users)
            {
                var contact = _ioc.Resolve<IContact>();
                contact.FillValues(item.FirstName, item.LastName, item.Phone);
                contacts.Add(contact);
            }
            Contacts = contacts;
        }
    }

    class DialogsService : IDialogsService
    {
        public IDialog Dialog { get; private set; }
        private ITelegramClient _client;

        private SimpleIoC _ioc;

        public DialogsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        private DateTime TimeUnixTOWindows(Double TimestampToConvert, bool Local)
        {
            var mdt = new DateTime(1970, 1, 1, 0, 0, 0);
            if (Local)
            {
                return mdt.AddSeconds(TimestampToConvert).ToLocalTime();
            }
            else
            {
                return mdt.AddSeconds(TimestampToConvert);
            }
        }

        private void AddMsg(TlMessage message, List<IMessage> messages)
        {
            var msg = _ioc.Resolve<IMessage>();
            msg.Fill("нужно подтянуть имя из FromId из Users от history", "dede", message.Message, TimeUnixTOWindows(message.Date, true));
            messages.Add(msg);
        }

        public async Task FillDialog(TlAbsPeer peer, string dialogName)
        {
            Dialog = _ioc.Resolve<IDialog>();
            List<IMessage> messages = new List<IMessage>();

            TlAbsMessages history;

            var dialogs = (TlDialogs) await _client.GetUserDialogsAsync();
            if (peer is TlPeerUser)
            {
                var user = dialogs.Users.Lists
                .OfType<TlUser>()
                .FirstOrDefault(c => c.FirstName + c.LastName == dialogName);
                history = await _client.GetHistoryAsync(new TlInputPeerUser() { UserId = user.Id }, 0, -1, 50);
                foreach (TlMessage message in ((TlMessagesSlice)history).Messages.Lists)
                {
                    AddMsg(message, messages);
                }
            }
            else
            {
                if (peer is TlPeerChannel)
                {
                    var chat = dialogs.Chats.Lists
                    .OfType<TlChannel>()
                    .FirstOrDefault(c => c.Title == dialogName);
                    history = await _client.GetHistoryAsync(new TlInputPeerChannel() { ChannelId = chat.Id, AccessHash= (long)chat.AccessHash }, 0, -1, 50);
                    foreach (TlMessage message in ((TlChannelMessages)history).Messages.Lists)
                    {
                        AddMsg(message, messages);
                    }
                }
                else
                {
                    var chat = dialogs.Chats.Lists
                    .OfType<TlChat>()
                    .FirstOrDefault(c => c.Title == dialogName);
                    history = await _client.GetHistoryAsync(new TlInputPeerChat() { ChatId = chat.Id }, 0, -1, 50);
                    foreach (TlMessage message in ((TlMessagesSlice)history).Messages.Lists)
                    {
                        AddMsg(message, messages);
                    }
                }
            }
            Dialog.Fill(dialogName, messages.Reverse<IMessage>());
        }
    }
    class Dialog : IDialog
    {
        public string DialogName { get; private set; }
        public IEnumerable<IMessage> Messages { get; private set; }

        public void Fill(string dialogName, IEnumerable<IMessage> messages)
        {
            DialogName = dialogName;
            Messages = messages;
        }
    }
    class Message : IMessage
    {
        public string UserFirstName { get; private set; }
        public string UserLastName { get; private set; }
        public string MessageText { get; private set; }
        public DateTime MessageDate { get; private set; }

        public void Fill(string userFirstName, string userLastName, string text, DateTime date)
        {
            UserFirstName = userFirstName;
            UserLastName = userLastName;
            MessageText = text;
            MessageDate = date;
        }
    }
    class Contact : IContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; private set; }

        public void FillValues(string firstName, string lastName, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phone;
        }
    }
}
