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
            TlAbsInputPeer receiver = await GetInputPeer(peer, id);
            await _client.SendMessageAsync(receiver, msg);
        }

        public async Task SendFile(Peer peer, int id, string path, string caption)
        {
            var reciever = await GetInputPeer(peer, id);
            path = path.Trim('"');
            var str = path.Split('\\');
            using (var stream = new FileStream(path, FileMode.Open))
            {
                //var fileResult = await _client.UploadFile(str[str.Length - 1], new StreamReader(stream));
                //await _client.SendUploadedPhoto(reciever, fileResult, caption);
                try
                {
                    var fileResult = await _client.UploadFile(str[str.Length - 1], new StreamReader(stream));
                    var attr = new TlVector<TlAbsDocumentAttribute>();
                    var filename = new TlDocumentAttributeFilename();
                    filename.FileName = str[str.Length - 1];
                    attr.Lists.Add(filename);
                    await _client.SendUploadedDocument(reciever, fileResult, caption, "", attr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sending failed! Try again");
                }

            }
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
