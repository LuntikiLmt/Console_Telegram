using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            string ownPhoneNumber = "79538909739"; //свой номер
            string recipientPhoneNumber = "79538909318"; //номер получателя
            string message = "Hello";
            try
            {
                service.Connect(ownPhoneNumber).Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Connection error");
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Enter the code:");
            string code = Console.ReadLine();
            try
            {
                service.Authenticate(code).Wait();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Authentication error");
                Console.WriteLine(ex.ToString());
            }
            try
            {
                service.ShowContacts();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Get contacts error");
                Console.WriteLine(ex.ToString());
            }
            //try
            //{
            //    service.SendMessage(recipientPhoneNumber, message).Wait();
            //}
            //catch (AggregateException ex)
            //{
            //    Console.WriteLine("Sending message error");
            //    Console.WriteLine(ex.ToString());
            //}
            Console.ReadKey();
        }
    }
    public interface IServiceTL : IContacts
    {
        Task Connect(string phoneNumber);
        Task Authenticate(string code);
        Task SendMessage(string WhomPhone, string Message);
    }
    public interface IConnection
    {
        Task Connect();
    }
    public interface IAuthentication
    {
        Task Authenticate();
    }
    public interface IContacts
    {
        IEnumerable<TLUser> Contacts { get; }
        Task ShowContacts();
    }
    public class Service : IServiceTL
    {
        public IEnumerable<TLUser> Contacts { get; private set; }
        private const int api_id = 35699;
        private const string api_hash = "c5faabe85e286bbb3eac32df78b34517";
        private TelegramClient client;
        private string phoneNumber;
        private string hash;

        public Service()
        {
            this.client = new TelegramClient(api_id, api_hash);
        }

        public async Task Connect(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
            await this.client.ConnectAsync();
            this.hash = await client.SendCodeRequestAsync(phoneNumber);
        }

        public async Task Authenticate(string code)
        {
            var user = await client.MakeAuthAsync(phoneNumber, hash, code);
        }
        public async Task ShowContacts()
        {
            var result = await client.GetContactsAsync();
            var users = result.users.lists
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>();
            Contacts = users;
            Console.WriteLine("All contacts:");
            foreach (var item in Contacts)
            {
                Console.WriteLine("{0} {1} {2}", item.first_name, item.last_name, item.username);
            }
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
