using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;

namespace ConsoleTelegram
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Service();

            bot.Connect().Wait();
            bot.Authenticate("79538909739").Wait();
            bot.SendMes("79538909318", "Hello epta").Wait();
        }
    }

    public sealed class Service
    {
        private TelegramClient client;
        FileSessionStore store = new FileSessionStore();

        public Service()
        {
            this.client = new TelegramClient(158868, "ece1a6343e8c1ec89502987b89c68e28", store);
        }

        public async Task Connect()
        {
            await this.client.ConnectAsync();
        }

        public async Task Authenticate(String phoneNumber)
        {
            var hash = await client.SendCodeRequestAsync(phoneNumber);

            //{
            //    Debugger.Break();
            //}

            var code = "12345"; // you can change code in debugger

            var user = await client.MakeAuthAsync(phoneNumber, hash, code);
            var s = client.IsUserAuthorized();
        }

        public async Task SendMes(string WhomPhone, string Message)
        {
            var result = await client.GetContactsAsync();

            //find recipient in contacts
            var user = result.users.lists
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast <TLUser>()
                .FirstOrDefault(x => x.phone == WhomPhone);

            //send message
            await client.SendMessageAsync(new TLInputPeerUser() { user_id = user.id }, Message);
        }
    }
}
