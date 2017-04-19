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
        void FillContacts();
    }
    public interface IDialogsService
    {
        IEnumerable<IDialog> Dialogs { get; }
        void FillDialogs();
    }
   
    public interface ISending
    {
        void SendTextMessage();
        void SendFile();
    }
    public interface IReceiving
    {
        void Receieve();
    }

    public interface IServiceTL
    {
        IContactsService ContactServise { get; set; }
        IDialogsService DialogsService { get; set; }
        ISending Sending { get; set; }
        IReceiving Receiving { get; set; }
    }
}
