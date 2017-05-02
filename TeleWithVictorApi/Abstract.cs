using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    public enum Peer { User, Channel, Chat }

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

    public interface IDialogShort
    {
        string DialogName { get; }
        Peer Peer { get; }
        int Id { get; }
        void Fill(string DlName, Peer DlPeer, int DlId);
    }
    
    public interface IContactsService
    {
        IEnumerable<IContact> Contacts { get; }
        Task FillContacts();
    }

    public interface IDialogsService
    {
        IDialog Dialog { get; }
        Task FillDialog(string dialogName, Peer peer, int id);//TlAbsPeer peer - тип диалога: chat, channel, user

        IEnumerable<IDialogShort> DialogList { get; }
        Task FillDialogList();
    }
   
    public interface ISendingService
    {
        Task SendTextMessage(Peer peer, int id, string msg);
        Task SendFile();
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

        int Fill();
    }
}
