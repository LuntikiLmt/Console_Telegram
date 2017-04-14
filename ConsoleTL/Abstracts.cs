using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTL
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
    //public interface IContacts
    //{

    //}
    public interface IContactsService
    {
        IEnumerable<IContact> Contacts { get; }
        void ShowContacts();
    }
    public interface IDialogsService
    {
        IEnumerable<IDialog> Dialogs { get; }
        void GetDialogs();
    }
    //public interface ISearch
    //{
    //    void Search();
    //}
    //public interface IAuth
    //{
    //    void Authenticate();
    //}
    public interface ISending
    {
        void SendTextMessage();
        void SendFile();
    }
    public interface IReceiving
    {
        void Receieve();
    }
}
