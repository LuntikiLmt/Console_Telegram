using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            DialogsService.FillDialog("Лунтики").Wait();
            Console.WriteLine(DialogsService.Dialog.DialogName); 
            foreach (var item in DialogsService.Dialog.Messages)
            {
                Console.WriteLine(item.MessageDate + " from " + item.UserFirstName + " " + item.UserLastName + ": " + item.MessageText);
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
