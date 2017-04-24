using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string UserName { get; }
        string MessageText { get; }
        DateTime MessageDate { get; }
    }
    public interface IDialog
    {
        string DialogName { get; }
        IEnumerable<IMessage> Messages { get; }
    }
    
    public interface IContactsService
    {
        IEnumerable<IContact> Contacts { get; }
        Task FillContacts();
    }
    public interface IDialogsService
    {
        IEnumerable<IDialog> Dialogs { get; }
        Task FillDialogs();
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
