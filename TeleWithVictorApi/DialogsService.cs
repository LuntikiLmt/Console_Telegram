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
                    foreach (var user in history.Users.Lists)
                    {
                        if ((user as TlUser)?.Id == msg.FromId)
                        {
                            userFrom = user;
                            break;
                        }
                    }
                    AddMsg(msg, _messages, userFrom == null ? dialogName : $"{userFrom.FirstName} {userFrom.LastName}");
                }
            }

            Dialog.Fill(dialogName, _messages.Reverse<IMessage>());
        }

        private void AddMsg(TlMessage message, List<IMessage> messages, string senderName)
        {
            var msg = _ioc.Resolve<IMessage>();
            msg.Fill(senderName, message.Message, DateTimeService.TimeUnixToWindows(message.Date, true));
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
                switch (dlg.Peer)
                {
                    case TlPeerUser peerUser:
                        id = peerUser.UserId;
                        peer = Peer.User;
                        var user = dialogs.Users.Lists
                            .OfType<TlUser>()
                            .FirstOrDefault(c => c.Id == id);
                        title = user?.FirstName + " " + user?.LastName;
                        break;
                    case TlPeerChannel peerChannel:
                        id = peerChannel.ChannelId;
                        peer = Peer.Channel;
                        title = dialogs.Chats.Lists
                            .OfType<TlChannel>()
                            .FirstOrDefault(c => c.Id == id)?.Title;
                        break;
                    case TlPeerChat peerChat:
                        id = peerChat.ChatId;
                        peer = Peer.Chat;
                        title = dialogs.Chats.Lists
                            .OfType<TlChat>()
                            .FirstOrDefault(c => c.Id == id)?.Title;
                        break;
                    default:
                        id = -1;
                        peer = Peer.Unknown;
                        title = String.Empty;
                        break;
                }
                var dlgShort = _ioc.Resolve<IDialogShort>();
                dlgShort.Fill(title, peer, id);
                dialogsShort.Add(dlgShort);
            }
            DialogList = dialogsShort;
        }
    }

    class Dialog : IDialog
    {
        public string DialogName { get; private set; }
        public IEnumerable<IMessage> Messages { get; private set; }

        public void Fill(string dialogName, IEnumerable<IMessage> messages)
        {
            DialogName = dialogName;
            Messages = messages;
        }
    }
    class Message : IMessage
    {
        public string SenderName { get; private set; }
        public string MessageText { get; private set; }
        public DateTime MessageDate { get; private set; }

        public void Fill(string senderName, string text, DateTime date)
        {
            SenderName = senderName;
            MessageText = text;
            MessageDate = date;
        }

        public override string ToString()
        {
            return $"{MessageDate} from {SenderName}: {MessageText}";
        }
    }
    class DialogShort : IDialogShort
    {
        public string DialogName { get; private set; }
        public Peer Peer { get; private set; }
        public int Id { get; private set; }

        public void Fill(string dlName, Peer dlPeer, int dlId)
        {
            DialogName = dlName;
            Peer = dlPeer;
            Id = dlId;
        }
    }
}
