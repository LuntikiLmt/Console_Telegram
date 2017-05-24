using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Messages;

namespace TeleWithVictorApi
{
    class DialogsService : IDialogsService
    {
        private readonly ITelegramClient _client;
        private readonly SimpleIoC _ioc;
        
        public IDialog Dialog { get; private set; }
        public IEnumerable<IDialogShort> DialogList { get; private set; }

        public DialogsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task FillDialog(string dialogName, Peer peer, int id)
        {
            List<IMessage> _messages = new List<IMessage>();

            Dialog = _ioc.Resolve<IDialog>();
            dynamic history;

            switch (peer)
            {
                case Peer.User:
                    history = await _client.GetHistoryAsync(new TlInputPeerUser { UserId = id }, 0, -1, 50);
                    break;

                case Peer.Chat:
                    history = await _client.GetHistoryAsync(new TlInputPeerChat { ChatId = id }, 0, -1, 50);
                    break;

                default:
                    var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
                    var channel = dialogs.Chats.Lists.OfType<TlChannel>().FirstOrDefault(c => c.Id == id);
                    history = await _client.GetHistoryAsync(new TlInputPeerChannel { ChannelId = channel.Id, AccessHash = (long)channel.AccessHash }, 0, -1, 50);
                    break;
            }

            foreach (var message in history.Messages.Lists)
            {
                var msg = message as TlMessage;
                if (msg != null)
                {
                    TlUser userFrom = null;
                    foreach (var list in history.Users.Lists)
                    {
                        if ((list as TlUser)?.Id == msg.FromId)
                        {
                            userFrom = list;
                            break;
                        }
                    }
                    if (userFrom == null)
                    {
                        AddMsg(msg, _messages, dialogName, "");
                    }
                    else
                    {
                        AddMsg(msg, _messages, userFrom.FirstName, userFrom.LastName);
                    }
                }
            }

            Dialog.Fill(dialogName, _messages.Reverse<IMessage>());
        }

        private void AddMsg(TlMessage message, List<IMessage> messages, string firstName, string lastName)
        {
            var msg = _ioc.Resolve<IMessage>();
            msg.Fill(firstName, lastName, message.Message, DateTimeService.TimeUnixToWindows(message.Date, true));
            messages.Add(msg);
        }

        public async Task FillDialogList()
        {
            List<IDialogShort> dialogsShort = new List<IDialogShort>();

            var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
            foreach (var dlg in dialogs.Dialogs.Lists)
            {
                int id;
                Peer peer;
                string title;
                var pr = dlg.Peer;
                if (pr is TlPeerUser)
                {
                    id = ((TlPeerUser)pr).UserId;
                    peer = Peer.User;
                    var user = dialogs.Users.Lists
                    .OfType<TlUser>()
                    .FirstOrDefault(c => c.Id == id);
                    title = user.FirstName + " " + user.LastName;
                }
                else
                {
                    if (pr is TlPeerChannel)
                    {
                        id = ((TlPeerChannel)pr).ChannelId;
                        peer = Peer.Channel;
                        title = dialogs.Chats.Lists
                        .OfType<TlChannel>()
                        .FirstOrDefault(c => c.Id == id).Title;
                    }
                    else
                    {
                        id = ((TlPeerChat)pr).ChatId;
                        peer = Peer.Chat;
                        title = dialogs.Chats.Lists
                        .OfType<TlChat>()
                        .FirstOrDefault(c => c.Id == id).Title;
                    }
                }
                var dlgShort = _ioc.Resolve<IDialogShort>();
                dlgShort.Fill(title, peer, id);
                dialogsShort.Add(dlgShort);
            }
            DialogList = dialogsShort;
        }
    }
}
