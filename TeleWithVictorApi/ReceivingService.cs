using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Messages;
using TelegramClient.Entities.TL.Updates;
using TelegramClient.Entities.TL.Upload;

namespace TeleWithVictorApi
{
    class ReceivingService : IReceivingService
    {
        private readonly ITelegramClient _client;
        private readonly SimpleIoC _ioc;

        public Stack<IMessage> UnreadMessages { get; } = new Stack<IMessage>();
        public event Action OnUpdateDialogs;
        public event Action OnUpdateContacts;
        public event Action<int, IMessage> OnAddUnreadMessage;
        //public event Action<string, string, DateTime> OnAddUnreadMessageFromChannel;

        public ReceivingService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
            _client.Updates.RecieveUpdates += Updates_RecieveUpdates;
        }

        private async void Updates_RecieveUpdates(TlAbsUpdates update)
        {
            switch (update)
            {
                case TlUpdateShort _:
                    break;

                case TlUpdates updates:
                    int id;
                    string text;
                    DateTime time;
                    foreach (var item in updates.Updates.Lists)
                    {
                        switch (item)
                        {
                            case TlUpdateDeleteMessages _:
                                OnUpdateDialogs?.Invoke();
                                break;

                            case TlUpdateContactLink _:
                                OnUpdateDialogs?.Invoke();
                                OnUpdateContacts?.Invoke();
                                break;

                            case TlUpdateNewChannelMessage updateNewChannelMessage:
                                var tlMessage = updateNewChannelMessage.Message as TlMessage;
                                text = tlMessage?.Message;
                                time = tlMessage.TimeUnixToWindows(true);
                                id = tlMessage.GetSenderId();
                                
                                AddNewMessageToUnread(id, text, time);
                                
                                break;

                            case TlUpdateNewMessage updateNewMessage:
                                id = (updateNewMessage.Message as TlMessage).GetSenderId();
                                text = (updateNewMessage.Message as TlMessage).GetTextMessage();
                                time = (updateNewMessage.Message as TlMessage).TimeUnixToWindows(true);
                                AddNewMessageToUnread(id, text, time);

                                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\Downloads");

                                switch ((updateNewMessage.Message as TlMessage).Media)
                                {
                                    case TlMessageMediaDocument document:
                                        var file = document.Document as TlDocument;
                                        var fileName = file.Attributes.Lists.OfType<TlDocumentAttributeFilename>().FirstOrDefault().FileName;

                                        int blockNumber = file.Size % 1048576 == 0 ? file.Size / 1048576 : file.Size / 1048576 + 1;
                                        List<byte> bytes = new List<byte>();
                                        for (int i = 0; i < blockNumber; i++)
                                        {
                                            var resFile = await _client.GetFile(new TlInputDocumentFileLocation { Id = file.Id, AccessHash = file.AccessHash, Version = file.Version }, file.Size, i * 1048576);
                                            bytes.AddRange(resFile.Bytes);
                                        }

                                        ConsoleTelegramUI.WriteToFile(bytes.ToArray(), fileName);
                                        break;

                                    case TlMessageMediaPhoto photo:
                                        var filePhoto = photo.Photo as TlPhoto;
                                        var photoInfo = filePhoto.Sizes.Lists.OfType<TlPhotoSize>().Last();
                                        var tf = (TlFileLocation)photoInfo.Location;
                                        var resFilePhoto = await _client.GetFile(new TlInputFileLocation { LocalId = tf.LocalId, Secret = tf.Secret, VolumeId = tf.VolumeId}, 0);

                                        var date = (updateNewMessage.Message as TlMessage).TimeUnixToWindows(true).ToString();
                                        date = date.Replace(':', '-');
                                        string photoName = $"ConsoleTelegram_{date}.png";

                                        ConsoleTelegramUI.WriteToFile(resFilePhoto.Bytes, photoName);
                                        break;
                                }

                                
                                break;
                        }
                    }
                    break;

                case TlUpdateShortMessage shortMessage:
                    AddNewMessageToUnread(shortMessage.UserId, shortMessage.Message,
                        shortMessage.TimeUnixToWindows(true));
                    break;

                //case TlUpdateShortChatMessage chatMessage:
                //    //OnAddUnreadMessage(chatMessage.FromId, chatMessage.Message, DateTimeService.TimeUnixToWindows(chatMessage.Date, false), chatMessage.ChatId);
                //    Console.WriteLine("asdas");
                //    break;

                default:
                    Console.WriteLine($"Default: {update}");
                    SystemSounds.Hand.Play();
                    break;
            }
        }

        private async Task AddNewMessageToUnread(int senderId, string text, DateTime dateTime)
        {
            var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
            var dialog = dialogs.Dialogs.Lists[0];

            string title = "Unknown sender";

            switch (dialog.Peer)
            {
                case TlPeerUser peerUser:
                    var user = dialogs.Users.Lists
                        .OfType<TlUser>()
                        .FirstOrDefault(c => c.Id == peerUser.UserId);
                    title = $"{user?.FirstName} {user?.LastName}";
                    break;
                case TlPeerChannel peerChannel:
                    var channel = dialogs.Chats.Lists
                        .OfType<TlChannel>()
                        .FirstOrDefault(c => c.Id == peerChannel.ChannelId);
                    title = $"{channel.Title}";
                    break;
                case TlPeerChat peerChat:
                    var chat = dialogs.Chats.Lists
                        .OfType<TlChat>()
                        .FirstOrDefault(c => c.Id == peerChat.ChatId);
                    title = $"{chat.Title}";
                    break;
            }

            var message = _ioc.Resolve<IMessage>();
            
            message.FillValues(title, text, dateTime);
            UnreadMessages.Push(message);
            OnAddUnreadMessage?.Invoke(senderId, message);
        }
    }
}
