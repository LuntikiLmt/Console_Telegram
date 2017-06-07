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
    static class TelegramMessage
    {
        public static async void LastMessage(ITelegramClient client)
        {
            var dialogs = (TlDialogs)await client.GetUserDialogsAsync();
            var dialog = dialogs.Dialogs.Lists[0];

            string title;

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
                default:
                    title = "Unknown sender";
                    break;
            }
        }


        public static string GetTextMessage(this TlMessage message)
        {
            string text = String.Empty;
            if (message.Media != null)
            {
                switch (message.Media)
                {
                    case TlMessageMediaDocument document:
                        text = $"{(document.Document as TlDocument).Attributes.Lists.OfType<TlDocumentAttributeFilename>().FirstOrDefault().FileName} {document.Caption}";
                        break;
                    case TlMessageMediaPhoto photo:
                        text = $"[Photo] {photo.Caption}";
                        break;
                }
            }
            else
            {
                text = message.Message;
            }
            
            return text;
        }

        public static DateTime TimeUnixToWindows(this TlMessage message, bool isLocal)
        {
            return DateTimeService.TimeUnixToWindows(message.Date, isLocal);
        }

        public static DateTime TimeUnixToWindows(this TlUpdateShortMessage message, bool isLocal)
        {
            return DateTimeService.TimeUnixToWindows(message.Date, isLocal);
        }

        public static DateTime TimeUnixToWindows(this TlUpdateShortSentMessage message, bool isLocal)
        {
            return DateTimeService.TimeUnixToWindows(message.Date, isLocal);
        }

        public static int GetSenderId(this TlMessage tlMessage)
        {
            int id = tlMessage.FromId ?? -1;
            if (id == -1)
            {
                var receiver = tlMessage.ToId;
                switch (receiver)
                {
                    case TlPeerChannel channel:
                        id = channel.ChannelId;
                        break;
                    case TlPeerChat chat:
                        id = chat.ChatId;
                        break;
                    case TlPeerUser user:
                        id = user.UserId;
                        break;
                }
            }
            return id;
        }
    }
}
