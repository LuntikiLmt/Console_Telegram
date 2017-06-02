using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Messages;
using TelegramClient.Entities.TL.Updates;

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

        private void Updates_RecieveUpdates(TlAbsUpdates update)
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
