﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Model;

namespace SignalRChat.Services
{
    public class ChatRoomService :IChatRoomService
    {
        //? inja to dictionari zakhire shode mishe ba code first db zad
        private readonly Dictionary<Guid, ChatRoom> _roomInfo = new Dictionary<Guid, ChatRoom>();
        private readonly Dictionary<Guid, List<ChatMessage>> _messageHistory = new Dictionary<Guid, List<ChatMessage>>();
        public Task<Guid> CreateRoom(string connectionId)
        {
            var id = Guid.NewGuid();
            _roomInfo[id] = new ChatRoom()
            {
                OwnerConnectionId = connectionId
            };

            return Task.FromResult(id);
        }

        public Task<Guid> GetRoomForConnectionId(string connectionId)
        {
            var foundRoom = _roomInfo.FirstOrDefault(x => x.Value.OwnerConnectionId == connectionId);

            if (foundRoom.Key == Guid.Empty)
            {
                throw new ArgumentException("Invalid Connection ID");
            }

            return Task.FromResult(foundRoom.Key);
        }

        public Task SetRoomName(Guid roomId, string name)
        {
            if (!_roomInfo.ContainsKey(roomId))
            {
                throw new ArgumentException("Invalid Room ID");
            }

            _roomInfo[roomId].Name = name;

            return Task.CompletedTask;
        }

        public Task AddMessage(Guid roomId, ChatMessage message)
        {
            if (!_messageHistory.ContainsKey(roomId))
            {
                _messageHistory[roomId] = new List<ChatMessage>();
            }
            _messageHistory[roomId].Add(message);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ChatMessage>> GetMessageHistory(Guid roomId)
        {
            //! inja mishe taghir dad mongo db pishnehad mishe bekhatere dserialize serialize dbcontext injoor chiza
            _messageHistory.TryGetValue(roomId, out var messages);

            messages = messages ?? new List<ChatMessage>();
            var sortedMessages = messages.OrderBy(x => x.SendAt)
                .AsEnumerable();

            return Task.FromResult(sortedMessages);
        }

        public Task<IReadOnlyDictionary<Guid, ChatRoom>> GetAllRooms()
        {
            return Task.FromResult(_roomInfo as IReadOnlyDictionary<Guid, ChatRoom>);
        }
    }
}
