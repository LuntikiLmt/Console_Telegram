using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    public interface IContact
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string PhoneNumber { get; }

        void FillValues(string firstName, string lastName, string phone);
    }
    public interface IMessage
    {
        string UserFirstName { get; }
        string UserLastName { get; }
        string MessageText { get; }
        //МОЖЕТ ЕЩЕ быть Медиа файл!!!
        DateTime MessageDate { get; }
        void Fill(string userFirstName, string userLastName, string text, DateTime date);
    }
    public interface IDialog
    {
        string DialogName { get; }
        IEnumerable<IMessage> Messages { get; }
        void Fill(string dialogName, IEnumerable<IMessage> messages);
    }
    
    public interface IContactsService
    {
        IEnumerable<IContact> Contacts { get; }
        Task FillContacts();
    }
    public interface IDialogsService
    {
        IDialog Dialog { get; }
        Task FillDialog(TlAbsPeer peer, string dialogName);//TlAbsPeer peer - тип диалога: chat, channel, user
    }
   
    public interface ISendingService
    {
        void SendTextMessage();
        void SendFile();
    }
    public interface IReceivingService
    {
        void Receieve();
    }

    public interface IServiceTL
    {
        IContactsService ContactsService { get; set; }
        IDialogsService DialogsService { get; set; }
        ISendingService SendingService { get; set; }
        IReceivingService ReceivingService { get; set; }

        void Fill();
    }
}
