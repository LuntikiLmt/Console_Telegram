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
        public event Action<int, IMessage> OnAddUnreadMessageFromUser;
        public event Action<string, string, DateTime> OnAddUnreadMessageFromChannel;

        public ReceivingService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
            _client.Updates.RecieveUpdates += Updates_RecieveUpdates;
        }

        async Task WriteToFile(byte[] bytes, string fileName)
        {
            try
            {
                using (FileStream fs = File.Create($"{Directory.GetCurrentDirectory()}\\Downloads\\{fileName}"))
                {
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    fs.Close();
                    Console.WriteLine($"{fileName} successfully installed in {fs.Name}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"saving of {fileName} failed!");
            }
        }

        private async void Updates_RecieveUpdates(TlAbsUpdates update)
        {
            switch (update)
            {
                case TlUpdateShort updateShort:
                    break;

                case TlUpdates updates:
                    SystemSounds.Beep.Play();
                    foreach (var item in updates.Updates.Lists)
                    {
                        switch (item)
                        {
                            case TlUpdateDeleteMessages updateDeleteMessages:
                                OnUpdateDialogs?.Invoke();
                                break;

                            case TlUpdateContactLink updateContactLink:
                                OnUpdateDialogs?.Invoke();
                                OnUpdateContacts?.Invoke();
                                break;

                            case TlUpdateNewChannelMessage updateNewChannelMessage:
                                var channel = updates.Chats.Lists.OfType<TlChannel>();
                                foreach (TlUpdateNewChannelMessage message in updates.Updates.Lists.OfType<TlUpdateNewChannelMessage>())
                                {
                                    OnAddUnreadMessageFromChannel?.Invoke(channel.ElementAt(0).Title,
                                        (message.Message as TlMessage).Message,
                                        DateTimeService.TimeUnixToWindows((message.Message as TlMessage).Date, false));
                                }
                                break;

                            case TlUpdateNewMessage updateNewMessage:
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

                                        WriteToFile(bytes.ToArray(), fileName);
                                        break;

                                    case TlMessageMediaPhoto photo:
                                        var filePhoto = photo.Photo as TlPhoto;
                                        var photoInfo = filePhoto.Sizes.Lists.OfType<TlPhotoSize>().Last();
                                        var tf = (TlFileLocation)photoInfo.Location;
                                        var resFilePhoto = await _client.GetFile(new TlInputFileLocation { LocalId = tf.LocalId, Secret = tf.Secret, VolumeId = tf.VolumeId}, 0);

                                        var date = DateTimeService.TimeUnixToWindows((updateNewMessage.Message as TlMessage).Date, true).ToString();
                                        date = date.Replace(':', '-');
                                        string photoName = $"ConsoleTelegram_{date}.png";

                                        WriteToFile(resFilePhoto.Bytes, photoName);
                                        break;
                                }
                                
                                break;
                        }
                    }
                    break;

                case TlUpdateShortMessage shortMessage:
                    Console.Beep();
                    AddNewMessageToUnread(shortMessage.UserId, shortMessage.Message,
                        DateTimeService.TimeUnixToWindows(shortMessage.Date, true)).Start();
                    break;

                //case TlUpdateShortChatMessage chatMessage:
                //    //OnAddUnreadMessage(chatMessage.FromId, chatMessage.Message, DateTimeService.TimeUnixToWindows(chatMessage.Date, false), chatMessage.ChatId);
                //    Console.WriteLine("asdas");
                //    break;

                default:
                    Console.WriteLine("Default: "+update);
                    SystemSounds.Hand.Play();
                    break;
            }
        }

        private async Task AddNewMessageToUnread(int id, string text, DateTime dateTime)
        {
            var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();
            var user = dialogs.Users.Lists.OfType<TlUser>().FirstOrDefault(c => c.Id == id);
            
            var message = _ioc.Resolve<IMessage>();
            
            message.Fill($"{user?.FirstName} {user?.LastName}", text, dateTime);
            UnreadMessages.Push(message);
            OnAddUnreadMessageFromUser?.Invoke(id, message);
        }
    }
}
