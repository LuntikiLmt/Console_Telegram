using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    class Options
    {
        public Options()
        {
            SendVerb = new SendOptions();
        }

        [VerbOption("send", HelpText = "Send message to somebody")]
        public SendOptions SendVerb { get; set; }
    }

    class SendOptions
    {
        [Option('t', "target", Required = true, HelpText = "Receiver")]
        public string Target { get; set; }

        [Option('m', "message", Required = true, HelpText = "Sending message")]
        public string Message { get; set; }
    }
}
