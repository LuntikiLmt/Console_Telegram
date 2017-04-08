using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;

namespace ConsoleTL
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioc = new SimpleIoC();
            #region RegisterBlock
            ioc.Register<IServiceTL, Service>();
            #endregion

            var service = ioc.Resolve<IServiceTL>();

            string ownPhoneNumber = "+79111111111";
            string recipientPhoneNumber = "+79111111112";
            string message = "Hello";
            try
            {
                service.Connect().Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Connection error");
                Console.WriteLine(ex.ToString());
            }
            try
            {
                service.Authenticate(ownPhoneNumber).Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Authentication error");
                Console.WriteLine(ex.ToString());
            }
            try
            {
                service.SendMessage(recipientPhoneNumber, message).Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Sending message error");
                Console.WriteLine(ex.ToString());
            }
        }
    }
    public interface IServiceTL
    {
        Task Connect();
        Task Authenticate(string phoneNumber);
        Task SendMessage(string WhomPhone, string Message);
    }

    public class Service : IServiceTL
    {
        private const int api_id = 123456;          //свой api_id
        private const string api_hash = "api_hash"; //свой api_hash
        private TelegramClient client;

        public Service()
        {
            this.client = new TelegramClient(api_id, api_hash);
        }

        public async Task Connect()
        {
            await this.client.ConnectAsync();
        }

        public async Task Authenticate(String phoneNumber)
        {
            var hash = await client.SendCodeRequestAsync(phoneNumber);
            var code = "12345"; // you can change code in debugger

            var user = await client.MakeAuthAsync(phoneNumber, hash, code);
            var s = client.IsUserAuthorized();
        }

        public async Task SendMessage(string whomPhone, string message)
        {
            var result = await client.GetContactsAsync();

            //find recipient in contacts
            var user = result.users.lists
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .FirstOrDefault(x => x.phone == whomPhone);

            //send message
            await client.SendMessageAsync(new TLInputPeerUser() { user_id = user.id }, message);
        }
    }
}
