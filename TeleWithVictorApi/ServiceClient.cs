using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.Exceptions;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    class ServiceClient : IServiceTL
    {
        private const int ApiId = 35699;
        private const string ApiHash = "c5faabe85e286bbb3eac32df78b34517";
        string ServerAddress = "149.154.167.50";
        int ServerPort = 443;
        private ITelegramClient client;

        public IContactsService ContactServise { get; set; }
        public IDialogsService DialogsService { get ; set; }
        public ISending Sending { get; set; }
        public IReceiving Receiving { get; set; }

        public  ServiceClient()
        {
            Authenticate().Wait();

            //AuthUser().Wait();
        }

        private bool Validate_InputPhone(string phone)
        {
            if (phone[0] == '7' && phone.Length == 11)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
        private async Task Authenticate()
        {
            client = ClientFactory.BuildClient(ApiId, ApiHash, ServerAddress, ServerPort);
            await client.ConnectAsync();
            if (!client.IsUserAuthorized())
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
                    hash = await client.SendCodeRequestAsync(phoneNumber);
                }
                catch (Exception e)
                {

                }

                string code;
                bool isCodeCorrect = false;
                while (!isCodeCorrect)
                {
                    code = Console.ReadLine();// you can change code in debugger
                    try
                    {
                        var user = await client.MakeAuthAsync(phoneNumber, hash, code);
                        
                        isCodeCorrect = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Code is incorrect! Try again please: ");
                        Console.WriteLine(e.ToString());
                    }
                }
                Console.WriteLine("Welcome!");
            }
        }
        //____________Изменение_________________

        //---------------------------2_VAriant-------------------------
        private ITelegramClient NewClient()
        {
            try
            {
                return ClientFactory.BuildClient(ApiId, ApiHash, ServerAddress, ServerPort);
            }
            catch (MissingApiConfigurationException ex)
            {
                throw new Exception(
                    $"Please add your API settings to the `appsettings.json` file. (More info: {MissingApiConfigurationException.InfoUrl})",
                    ex);
            }
        }
        public virtual async Task AuthUser()
        {
            var client = NewClient();

            try
            {
                await client.ConnectAsync();
            }
            catch (Exception e)
            {
                var s = e.ToString();
            }

            var hash = await client.SendCodeRequestAsync("79538909318");
            var code = Console.ReadLine();  // you can change code in debugger too

            if (string.IsNullOrWhiteSpace(code))
                throw new Exception(
                    "CodeToAuthenticate is empty in the appsettings.json file, fill it with the code you just got now by SMS/Telegram");

            TlUser user;
            try
            {
                user = await client.MakeAuthAsync("79538909318", hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                //var password = await client.GetPasswordSetting();
                //var passwordStr = PasswordToAuthenticate;

                //user = await client.MakeAuthWithPasswordAsync(password, passwordStr);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception(
                    "CodeToAuthenticate is wrong in the appsettings.json file, fill it with the code you just got now by SMS/Telegram",
                    ex);
            }
            //Assert.NotNull(user);
            //Assert.True(client.IsUserAuthorized());
            Console.WriteLine("fwefwfwefe");
        }
    }
}
