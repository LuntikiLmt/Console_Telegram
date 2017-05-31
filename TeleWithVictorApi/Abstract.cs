using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;

namespace TeleWithVictorApi
{
    public enum Peer { User, Channel, Chat, Unknown }

    public interface IHaveId
    {
        int Id { get; set; }
    }

    public interface IContact : IHaveId
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string PhoneNumber { get; }

        void FillValues(string firstName, string lastName, string phone);
    }

    public interface IMessage
    {
        string SenderName { get; }
        string MessageText { get; }
        //МОЖЕТ ЕЩЕ быть Медиа файл!!!
        DateTime MessageDate { get; }
        void Fill(string senderName, string text, DateTime date);
    }

    public interface IDialog
    {
        string DialogName { get; }
        IEnumerable<IMessage> Messages { get; }
        void Fill(string dialogName, IEnumerable<IMessage> messages);
    }

    public interface IDialogShort : IHaveId
    {
        string DialogName { get; }
        Peer Peer { get; }

        void Fill(string dlName, Peer dlPeer);
    }
    
    public interface IContactsService
    {
        IEnumerable<IContact> Contacts { get; }
        Task FillContacts();
        Task AddContact(string firstName, string lastName, string phone);
        Task DeleteContact(int number);
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
        List<IMessage> UnreadMessages { get; }
        event Action OnUpdateDialogs;
        event Action OnUpdateContacts;
        event Action<int, string, DateTime> OnAddUnreadMessageFromUser;
        event Action<string, string, DateTime> OnAddUnreadMessageFromChannel;
    }

    public interface IServiceTl : IAuthorization
    {
        IContactsService ContactsService { get; }
        IDialogsService DialogsService { get; }
        ISendingService SendingService { get; }
        IReceivingService ReceivingService { get; }

        Task FillAsync();
    }

    public interface IAuthorization
    {
        void LogOut();
        bool Authorize();
        Task EnterPhoneNumber(string number);
        Task<bool> EnterIncomingCode(string code);
    }
}
