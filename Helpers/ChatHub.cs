using HRMSystem.Data;
using HRMSystem.DTOs;
using HRMSystem.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Helpers
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                conversationId.ToString()
            );
        }

        public async Task SendMessage(int conversationId, SendMessageDto dto)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = dto.SenderId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", message);
        }
    }
}
