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
        private ITelegramClient _client;
        private SimpleIoC _ioc;
        private List<IMessage> _messages = new List<IMessage>();
        public IDialog Dialog { get; private set; }
        public IEnumerable<IDialogShort> DialogList { get; private set; }

        public DialogsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task FillDialog(string dialogName, Peer peer, int id)
        {
            Dialog = _ioc.Resolve<IDialog>();
            TlAbsMessages history;
            TlAbsMessages temp;
            int len;

            switch (peer)
            {
                case Peer.User:
                    history = await _client.GetHistoryAsync(new TlInputPeerUser() { UserId = id }, 0, -1, 50);
                    if (history is TlMessagesSlice)
                    {
                        foreach (TlMessage message in ((TlMessagesSlice)history).Messages.Lists)
                        {
                            TlUser userFrom = ((TlMessagesSlice)history).Users.Lists
                            .OfType<TlUser>()
                            .FirstOrDefault(c => c.Id == message.FromId);
                            AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                        }
                    }
                    else
                    {
                        foreach (TlMessage message in ((TlMessages)history).Messages.Lists)
                        {
                            TlUser userFrom = ((TlMessages)history).Users.Lists
                            .OfType<TlUser>()
                            .FirstOrDefault(c => c.Id == message.FromId);
                            AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                        }
                    }
                    //len = ((TlMessages) temp).Messages.Lists.Count - 1;
                    //history = (TlMessagesSlice)await _client.GetHistoryAsync(new TlInputPeerUser() { UserId = id }, 0, -1, len < 50 ? len: 50);

                    break;

                case Peer.Chat:
                    history = await _client.GetHistoryAsync(new TlInputPeerChat() { ChatId = id }, 0, -1, 50);
                    foreach (TlMessage message in ((TlMessagesSlice)history).Messages.Lists)
                    {
                        TlUser userFrom = ((TlMessagesSlice)history).Users.Lists
                        .OfType<TlUser>()
                        .FirstOrDefault(c => c.Id == message.FromId);
                        AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                    }
                    break;

                default:
                    var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
                    var channel = dialogs.Chats.Lists.OfType<TlChannel>().FirstOrDefault(c => c.Id == id);
                    history = (TlChannelMessages)await _client.GetHistoryAsync(new TlInputPeerChannel() { ChannelId = channel.Id, AccessHash = (long)channel.AccessHash }, 0, -1, 50);
                    foreach (TlMessage message in ((TlChannelMessages)history).Messages.Lists)
                    {
                        TlUser userFrom = ((TlChannelMessages)history).Users.Lists
                        .OfType<TlUser>()
                        .FirstOrDefault(c => c.Id == message.FromId);
                        AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                    }
                    break;
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
