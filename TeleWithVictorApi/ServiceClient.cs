using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    class ServiceClient : IServiceTl
    {
        private readonly ITelegramClient _client;
        private readonly SimpleIoC _ioc;

        public IContactsService ContactsService { get; set; }
        public IDialogsService DialogsService { get ; set; }
        public ISendingService SendingService { get; set; }
        public IReceivingService ReceivingService { get; set; }

        public ServiceClient(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();

            Authenticate().Wait();
        }

        public async Task FillAsync()
        {
            DialogsService = _ioc.Resolve<IDialogsService>();
            SendingService = _ioc.Resolve<ISendingService>();
            ContactsService = _ioc.Resolve<IContactsService>();
            ReceivingService = _ioc.Resolve<IReceivingService>();
            ReceivingService.OnUpdateDialogs += ReceivingService_OnUpdateDialogs;
            ReceivingService.OnUpdateContacts += ReceivingService_OnUpdateContacts;

            await ContactsService.FillContacts();
            await DialogsService.FillDialogList();
        }

        private void ReceivingService_OnUpdateContacts()
        {
            ContactsService.FillContacts();
        }

        private void ReceivingService_OnUpdateDialogs()
        {
            DialogsService.FillDialogList();
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
            Console.WriteLine("\nWelcome!");
        }
    }
}
