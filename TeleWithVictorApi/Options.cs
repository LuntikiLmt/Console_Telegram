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

    [Verb("send", HelpText = "Send message to somebody")]
    class SendOptions : ISendOptions
    {
        public IEnumerable<string> Message { get; set; }
        public int Index { get; set; }
        public bool Dialog { get; set; }
        public bool Contact { get; set; }
    }

    interface IPrintOptions
    {
        [Option('d', "dialogs", Default = false)]
        bool Dialogs { get; set; }

        [Option('c', "contacts", Default = false)]
        bool Contacts { get; set; }
    }

    [Verb("print", HelpText = "Print list of contacts or dialogs")]
    class PrintOptions : IPrintOptions
    {
        public bool Dialogs { get; set; }
        public bool Contacts { get; set; }
    }

    [Verb("quit", HelpText = "Leave this pretty program")]
    class Quit
    {

    }
}
