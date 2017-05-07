﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Entities;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Contacts;

namespace TeleWithVictorApi
{
    class ContactsService : IContactsService
    {
        private readonly ITelegramClient _client;
        private readonly SimpleIoC _ioc;

        public IEnumerable<IContact> Contacts { get; private set; }

        public ContactsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task AddContact(string firstName, string lastName, string phone)
        {
            var contacts = new TlVector<TlInputPhoneContact>();
            contacts.Lists.Add(new TlInputPhoneContact {  FirstName = firstName, LastName = lastName, Phone = phone});

            //Create request 
            var req = new TlRequestImportContacts
            {
                Contacts = contacts
            };
            await _client.SendRequestAsync<TlImportedContacts>(req);
            await FillContacts();
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
                contact.FillValues(item.FirstName, item.LastName, item.Phone, item.Id);
                contacts.Add(contact);
            }
            Contacts = contacts;
        }
    }
}
