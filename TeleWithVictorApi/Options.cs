using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    interface ISendOptions
    {
        [Option('m', "message", Default = "Hello", HelpText = "Sending message", Separator = ' ')]
        IEnumerable<string> Message { get; set; }

        [Value(0, HelpText = "Index in dialogs list", Required = true)]
        int Index { get; set; }

        [Option('d', "dialog", Default = false, HelpText = "Send to dialog")]
        bool Dialog { get; set; }

        [Option('c', "contact", Default = false, HelpText = "Send to contact")]
        bool Contact { get; set; }
    }

    interface IPrintOptions
    {
        [Option('d', "dialogs", HelpText = "Dialog list", Default = false)]
        bool Dialogs { get; set; }

        [Option('c', "contacts", HelpText = "Contact list", Default = false)]
        bool Contacts { get; set; }

        [Option('m', "messages", HelpText = "Messages in dialog", Default = -1)]
        int Index { get; set; }
    }

    interface IAddContactOptions
    {
        [Value(0, HelpText = "Telephone number", Required = true)]
        string Number { get; set; }

        [Value(1, HelpText = "First name", Required = true)]
        string FirstName { get; set; }

        [Value(2, HelpText = "LastName", Required = false)]
        string LastName { get; set; }
    }

    [Verb("send", HelpText = "Send message to somebody")]
    class SendOptions : ISendOptions
    {
        public IEnumerable<string> Message { get; set; }
        public int Index { get; set; }
        public bool Dialog { get; set; }
        public bool Contact { get; set; }
    }

    [Verb("print", HelpText = "Print list of contacts or dialogs")]
    class PrintOptions : IPrintOptions
    {
        public bool Dialogs { get; set; }
        public bool Contacts { get; set; }
        public int Index { get; set; }
    }

    [Verb("addContact", HelpText = "Add contact to contact list")]
    class AddContactOptions : IAddContactOptions
    {
        public string Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Verb("quit", HelpText = "Leave this pretty program")]
    class Quit
    {

    }
}
