﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRChat.Model;
using SignalRChat.Services;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;

        private readonly IHubContext<AgentHub> _agentHub;

        public ChatHub(IChatRoomService chatRoomService , IHubContext<AgentHub> agenthub)
        {
            _chatRoomService = chatRoomService;
            _agentHub = agenthub;
        }
        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                await base.OnConnectedAsync();
                return;
            }

            var roomId = await _chatRoomService.CreateRoom(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync("ReceiveMessage",
                "Amethyst",
                DateTimeOffset.UtcNow,
                "Get the fuck out of here");

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string name, string text)
        {
            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SendAt = DateTimeOffset.Now
            };

            await _chatRoomService.AddMessage(roomId, message);


            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", message.SenderName, message.SendAt, message.Text);
        }

        public async Task SetName(string visitorName)
        {
            var roomName = $"Chat With {visitorName} from the web .";

            var roomId = await _chatRoomService.GetRoomForConnectionId(Context.ConnectionId);

            await _chatRoomService.SetRoomName(roomId, roomName);

            await _agentHub.Clients.All.SendAsync("ActiveRooms", await _chatRoomService.GetAllRooms());
        }

        [Authorize]
        public async Task JoinRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new ArgumentException("Invalid Room ID");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        [Authorize]
        public async Task LeaveRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
            {
                throw new ArgumentException("Invalid Room ID");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }
    }
}
