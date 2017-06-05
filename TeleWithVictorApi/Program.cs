using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Updates;

namespace TeleWithVictorApi
{
    class Program
    {
        

        static void Main(string[] args)
        {
            var ioc = new SimpleIoC();

            #region RegisterIoC
            ioc.RegisterInstance(TelegramClient.Core.ClientFactory.BuildClient(35699, "c5faabe85e286bbb3eac32df78b34517", "149.154.167.40", 443));
            ioc.Register<IContactsService, ContactsService>();
            ioc.Register<IDialogsService, DialogsService>();
            ioc.Register<ISendingService, SendingService>();
            ioc.Register<IReceivingService, ReceivingService>();

            ioc.Register<IServiceTl, ServiceClient>();

            ioc.Register<IConsoleTelegramUI, ConsoleTelegramUI>();

            ioc.Register<IContact, Contact>();
            ioc.Register<IDialog, Dialog>();
            ioc.Register<IMessage, Message>();
            ioc.Register<IDialogShort, DialogShort>();
            #endregion

            var ui = ioc.Resolve<IConsoleTelegramUI>();
            ui.Authorize();
            ui.Start();
        }
    }
}
