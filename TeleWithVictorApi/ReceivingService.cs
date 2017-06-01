using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using TelegramClient.Core;
using TelegramClient.Core.ApiServies;
using TelegramClient.Entities.TL;
using TelegramClient.Entities.TL.Updates;

namespace TeleWithVictorApi
{
    class ReceivingService : IReceivingService
    {
        private readonly ITelegramClient _client;

        public List<IMessage> UnreadMessages { get; private set; } = new List<IMessage>();
        public event Action OnUpdateDialogs;
        public event Action OnUpdateContacts;
        public event Action<int, string, DateTime> OnAddUnreadMessageFromUser;
        public event Action<string, string, DateTime> OnAddUnreadMessageFromChannel;

        public ReceivingService(SimpleIoC ioc)
        {
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
                    if (updates.Updates.Lists.Count(item => item.GetType() == typeof(TlUpdateDeleteMessages)) != 0)
                    {
                        OnUpdateDialogs();
                    }
                    if (updates.Updates.Lists.Count(item => item.GetType() == typeof(TlUpdateContactLink)) != 0)
                    {
                        OnUpdateDialogs();
                        OnUpdateContacts();
                    }
                    if (updates.Updates.Lists.Count(item => item.GetType() == typeof(TlUpdateNewChannelMessage)) != 0)
                    {
                        var channel = updates.Chats.Lists.OfType<TlChannel>();
                        var mes = updates.Updates.Lists.OfType<TlUpdateNewChannelMessage>();
                        foreach (TlUpdateNewChannelMessage item in mes)
                        {
                            OnAddUnreadMessageFromChannel(channel.ElementAt(0).Title, (item.Message as TlMessage).Message, DateTimeService.TimeUnixToWindows((item.Message as TlMessage).Date, false));
                        }
                    }
                    if (((updates.Updates.Lists[0] as TlUpdateNewMessage).Message as TlMessage).Media != null)
                    {
                        
                    }
                    break;

                case TlUpdateShortMessage shortMessage:
                    //SystemSounds.Beep.Play();
                    Console.Beep();
                    OnAddUnreadMessageFromUser(shortMessage.UserId, shortMessage.Message, DateTimeService.TimeUnixToWindows(shortMessage.Date, false));
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
    }
}
