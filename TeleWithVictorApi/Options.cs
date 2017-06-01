using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    interface IHaveDCIndexes
    {
        [Option('d', "dialog", Default = -1, HelpText = "Send to dialog")]
        int DialogIndex { get; set; }

        [Option('c', "contact", Default = -1, HelpText = "Send to contact")]
        int ContactIndex { get; set; }
    }

    interface ISendOptions : IHaveDCIndexes
    {
        [Option('m', "message", HelpText = "Sending message", Required = true, Separator = ' ')]
        IEnumerable<string> Message { get; set; }
    }

    interface ISendFileOptions : IHaveDCIndexes
    {
        [Option('f', "file", HelpText = "Sending file", Required = true, Separator = ' ')]
        string Path { get; set; }
    }

    interface IPrintOptions
    {
        [Option('d', "dialogs", HelpText = "Dialog list", Default = false)]
        bool Dialogs { get; set; }

        [Option('u', "unread", HelpText = "Unread messages", Default = false)]
        bool UnreadMessages { get; set; }

        [Option('c', "contacts", HelpText = "Contact list", Default = false)]
        bool Contacts { get; set; }

        [Option('m', "messages", HelpText = "Messages in dialog", Default = -1)]
        int Index { get; set; }
    }

    interface IDeleteContactOptions
    {
        [Value(0,HelpText = "Number of contact to delete", Required =true)]
        int Index { get; set; }
    }

    interface IAddContactOptions
    {
        [Value(0, HelpText = "Telephone number", Required = true)]
        string Number { get; set; }

        [Value(1, HelpText = "First name", Required = true)]
        string FirstName { get; set; }

        [Value(2, HelpText = "Last Name", Required = false)]
        string LastName { get; set; }
    }

    [Verb("send", HelpText = "Send message to somebody")]
    class SendOptions : ISendOptions
    {
        public IEnumerable<string> Message { get; set; }
        public int DialogIndex { get; set; }
        public int ContactIndex { get; set; }
    }

    [Verb("sendFile", HelpText = "Send file to somebody")]
    class SendFileOptions : ISendFileOptions
    {
        public int DialogIndex { get; set; }
        public int ContactIndex { get; set; }
        public string Path { get; set; }
    }

    [Verb("print", HelpText = "Print list of 1) contacts 2) dialogs 3) messages in certain dialog 4) unread messages")]
    class PrintOptions : IPrintOptions
    {
        public bool Dialogs { get; set; }
        public bool Contacts { get; set; }
        public int Index { get; set; }
        public bool UnreadMessages { get; set; }
    }

    [Verb("deleteContact", HelpText = "Delete contact")]
    class DeleteContactOptions : IDeleteContactOptions
    {
        public int Index { get; set; }
    }

    [Verb("addContact", HelpText = "Add contact to contact list")]
    class AddContactOptions : IAddContactOptions
    {
        public string Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Verb("logout", HelpText = "Log out")]
    class LogOutOptions
    {
        
    }

    [Verb("quit", HelpText = "Leave this pretty program")]
    class Quit
    {

    }
}
