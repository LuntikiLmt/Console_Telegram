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
    class SendingService : ISendingService
    {
        private readonly ITelegramClient _client;

        public SendingService(SimpleIoC ioc)
        {
            _client = ioc.Resolve<ITelegramClient>();
        }

        public Task SendFile()
        {
            throw new NotImplementedException();
        }

        public async Task SendTextMessage(Peer peer, int id, string msg)
        {
            TlAbsInputPeer receiver;
            switch (peer)
            {
                case Peer.User:
                    receiver = new TlInputPeerUser { UserId = id };
                    break;
                case Peer.Chat:
                    receiver = new TlInputPeerChat { ChatId = id };
                    break;
                default:
                    var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
                    var chat = dialogs.Chats.Lists
                             .OfType<TlChannel>()
                             .FirstOrDefault(c => c.Id == id);
                    receiver = new TlInputPeerChannel { ChannelId = id, AccessHash = chat.AccessHash.Value };
                    break;
            }
            await _client.SendMessageAsync(receiver, msg);
        }
    }
}
