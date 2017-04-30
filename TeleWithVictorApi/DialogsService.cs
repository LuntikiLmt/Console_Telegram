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
        enum Peer { User, Channel, Chat }
        private ITelegramClient _client;
        private SimpleIoC _ioc;
        private List<IMessage> _messages = new List<IMessage>();

        public IDialog Dialog { get; private set; }

        public DialogsService(SimpleIoC ioc)
        {
            _ioc = ioc;
            _client = ioc.Resolve<ITelegramClient>();
        }

        public async Task FillDialog(string dialogName)
        {
            Dialog = _ioc.Resolve<IDialog>();

            var dialogs = (TlDialogs)await _client.GetUserDialogsAsync();

            var user = dialogs.Users.Lists.OfType<TlUser>().FirstOrDefault(c => c.FirstName + " " + c.LastName == dialogName);
            var channel = dialogs.Chats.Lists.OfType<TlChannel>().FirstOrDefault(c => c.Title == dialogName);
            var chat = dialogs.Chats.Lists.OfType<TlChat>().FirstOrDefault(c => c.Title == dialogName);

            if (user != null)
            {
                //FillUserDialog(user);
                TlMessagesSlice history = (TlMessagesSlice)await _client.GetHistoryAsync(new TlInputPeerUser() { UserId = user.Id }, 0, -1, 50);
                foreach (TlMessage message in history.Messages.Lists)
                {
                    TlUser userFrom = history.Users.Lists
                    .OfType<TlUser>()
                    .FirstOrDefault(c => c.Id == message.FromId);
                    AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                }
            }
            else if (channel != null)
            {
                //FillChannelDialog(channel);
                TlChannelMessages history = (TlChannelMessages)await _client.GetHistoryAsync(new TlInputPeerChannel() { ChannelId = channel.Id, AccessHash = (long)channel.AccessHash }, 0, -1, 50);
                foreach (TlMessage message in (history as TlChannelMessages).Messages.Lists)
                {
                    TlUser userFrom = (history as TlChannelMessages).Users.Lists
                    .OfType<TlUser>()
                    .FirstOrDefault(c => c.Id == message.FromId);
                    AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                }
            }
            else if (chat != null)
            {
                //FillChatDialog(chat);
                TlMessagesSlice history = (TlMessagesSlice)await _client.GetHistoryAsync(new TlInputPeerChat() { ChatId = chat.Id }, 0, -1, 50);
                foreach (TlMessage message in history.Messages.Lists)
                {
                    TlUser userFrom = history.Users.Lists
                    .OfType<TlUser>()
                    .FirstOrDefault(c => c.Id == message.FromId);
                    AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
                }
            }
            else
            {
                throw new ArgumentException("No such dialog");
            }            

            Dialog.Fill(dialogName, _messages.Reverse<IMessage>());
        }

        //private async void FillUserDialog(TlUser user)
        //{
        //    TlMessagesSlice history = (TlMessagesSlice) await _client.GetHistoryAsync(new TlInputPeerUser() { UserId = user.Id }, 0, -1, 50);
        //    foreach (TlMessage message in history.Messages.Lists)
        //    {
        //        TlUser userFrom = history.Users.Lists
        //        .OfType<TlUser>()
        //        .FirstOrDefault(c => c.Id == message.FromId);
        //        AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
        //    }
        //}
        //private async void FillChannelDialog(TlChannel channel)
        //{
        //    TlChannelMessages history = (TlChannelMessages) await _client.GetHistoryAsync(new TlInputPeerChannel() { ChannelId = channel.Id, AccessHash = (long)channel.AccessHash }, 0, -1, 50);
        //    foreach (TlMessage message in (history as TlChannelMessages).Messages.Lists)
        //    {
        //        TlUser userFrom = (history as TlChannelMessages).Users.Lists
        //        .OfType<TlUser>()
        //        .FirstOrDefault(c => c.Id == message.FromId);
        //        AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
        //    }
        //}
        //private async void FillChatDialog(TlChat chat)
        //{
        //    TlMessagesSlice history = (TlMessagesSlice) await _client.GetHistoryAsync(new TlInputPeerChat() { ChatId = chat.Id }, 0, -1, 50);
        //    foreach (TlMessage message in history.Messages.Lists)
        //    {
        //        TlUser userFrom = history.Users.Lists
        //        .OfType<TlUser>()
        //        .FirstOrDefault(c => c.Id == message.FromId);
        //        AddMsg(message, _messages, userFrom.FirstName, userFrom.LastName);
        //    }
        //}

        private void AddMsg(TlMessage message, List<IMessage> messages, string firstName, string lastName)
        {
            var msg = _ioc.Resolve<IMessage>();
            msg.Fill(firstName, lastName, message.Message, DateTimeService.TimeUnixToWindows(message.Date, true));
            messages.Add(msg);
        }
    }
}
