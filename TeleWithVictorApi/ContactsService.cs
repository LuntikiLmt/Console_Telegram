using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
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
            IEnumerable<TlUser> users = cont.Users.Lists.Cast<TlUser>();
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
}
