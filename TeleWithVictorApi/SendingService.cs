using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.Utils;
using TelegramClient.Entities;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Messages;

namespace TeleWithVictorApi
{
    class SendingService : ISendingService
    {
        private readonly ITelegramClient _client;
        private SimpleIoC _ioc;

        public event Action<IMessage> OnSendMessage;

        public SendingService(SimpleIoC ioc)
        {
            _client = ioc.Resolve<ITelegramClient>();
            _ioc = ioc;
        }

        public async Task SendTextMessage(Peer peer, int receiverId, string msg)
        {
            TlAbsInputPeer receiver = await GetInputPeer(peer, receiverId);
            var update = await _client.SendMessageAsync(receiver, msg);
            OnSendMessage?.Invoke(GetMessage(update));
        }

        public async Task SendFile(Peer peer, int receiverId, string path, string caption)
        {
            var receiver = await GetInputPeer(peer, receiverId);
            path = path.Trim('"');
            var str = path.Split('\\');
            using (var stream = new FileStream(path, FileMode.Open))
            {
                //var fileResult = await _client.UploadFile(str[str.Length - 1], new StreamReader(stream));
                //await _client.SendUploadedPhoto(reciever, fileResult, caption);
                var fileResult = await _client.UploadFile(str[str.Length - 1], new StreamReader(stream));
                var attr = new TlVector<TlAbsDocumentAttribute>();
                var filename = new TlDocumentAttributeFilename { FileName = str[str.Length - 1] };
                attr.Lists.Add(filename);
                var update = await _client.SendUploadedDocument(receiver, fileResult, caption, String.Empty, attr);
                OnSendMessage?.Invoke(GetMessage(update));
            }
        }

        private IMessage GetMessage(TlAbsUpdates udAbsUpdates)
        {
            int senderId = 0;
            string text = String.Empty;
            DateTime time = DateTime.Now;
            switch (udAbsUpdates)
            {
                case TlUpdateShortMessage tlUpdateShortMessage:
                    text = tlUpdateShortMessage.Message;
                    time = tlUpdateShortMessage.TimeUnixToWindows(true);
                    break;
                case TlUpdates u:
                    foreach (var item in u.Updates.Lists)
                    {
                        switch (item)
                        {
                            case TlUpdateNewMessage updateNewMessage:
                                text = (updateNewMessage.Message as TlMessage).GetTextMessage();
                                time = (updateNewMessage.Message as TlMessage).TimeUnixToWindows(true);
                                break;
                            case TlUpdateNewChannelMessage updateNewChannelMessage:
                                text = (updateNewChannelMessage.Message as TlMessage)?.Message;
                                time = (updateNewChannelMessage.Message as TlMessage).TimeUnixToWindows(true);
                                break;
                        }
                    }

                    break;
            }
            var message = _ioc.Resolve<IMessage>();
            message.FillValues("You", text, time);
            return message;
        }

        private async Task<TlAbsInputPeer> GetInputPeer(Peer peer, int id)
        {
            TlAbsInputPeer receiver;
            switch (peer)
            {
                case Peer.User:
                    receiver = new TlInputPeerUser {UserId = id};
                    break;
                case Peer.Chat:
                    receiver = new TlInputPeerChat {ChatId = id};
                    break;
                default:
                    var dialogs = (TlDialogs) await _client.GetUserDialogsAsync();
                    var chat = dialogs.Chats.Lists
                        .OfType<TlChannel>()
                        .FirstOrDefault(c => c.Id == id);
                    receiver = new TlInputPeerChannel {ChannelId = id, AccessHash = chat.AccessHash.Value};
                    break;
            }
            return receiver;
        }
    }
}
