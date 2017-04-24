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

            DialogsService.FillDialogs().Wait();
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
        public IEnumerable<IDialog> Dialogs { get; set; }
        private ITelegramClient _client;

        public DialogsService(SimpleIoC ioc)
        {
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task FillDialogs()
        {
            var dialogs = (TlDialogs) await _client.GetUserDialogsAsync();
        }
    }
    class Dialog : IDialog
    {
        public string DialogName { get; }
        public IEnumerable<IMessage> Messages { get; }

        public Dialog(string dialogName, IEnumerable<IMessage> messages)
        {
            DialogName = dialogName;
            Messages = messages;
        }
    }
    class Message : IMessage
    {
        public string UserName { get; }
        public string MessageText { get; }
        public DateTime MessageDate { get; }

        public Message(string userName, string text, DateTime date)
        {
            UserName = userName;
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
