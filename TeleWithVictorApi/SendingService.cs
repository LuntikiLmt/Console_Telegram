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
        private ITelegramClient _client;
        private SimpleIoC _ioc;

        public SendingService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public Task SendFile()
        {
            throw new NotImplementedException();
        }

        public async Task SendTextMessage(Peer peer, int id, string msg)
        {
            TlAbsInputPeer reciever;
            switch (peer)
            {
                case Peer.User:
                    reciever = new TlInputPeerUser { UserId = id };
                    break;
                case Peer.Chat:
                    reciever = new TlInputPeerChat { ChatId = id };
                    break;
                default:
                    var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
                    var chat = dialogs.Chats.Lists
                             .OfType<TlChannel>()
                             .FirstOrDefault(c => c.Id == id);
                    reciever = new TlInputPeerChannel { ChannelId = id, AccessHash = chat.AccessHash.Value };
                    break;
            }
            await _client.SendMessageAsync(reciever, msg);
        }
    }
}
