using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRequest?> GetPendingAsync(int fromId, int toId)
        {
            return await _context.ChatRequests.FirstOrDefaultAsync(x =>
                x.FromEmployeeId == fromId &&
                x.ToEmployeeId == toId &&
                x.Status == "Pending");
        }

        public async Task<ChatRequest> CreateAsync(ChatRequest request)
        {
            _context.ChatRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task ApproveAsync(int requestId, int approverId)
        {
            var req = await _context.ChatRequests.FindAsync(requestId);
            if (req == null) return;

            req.Status = "Approved";
            req.ApprovedBy = approverId;
            req.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<ChatRequest?> GetByIdAsync(int id)
        {
            return await _context.ChatRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<ChatRequest>> GetPendingForReceiverAsync(int toEmployeeId)
        {
            return await _context.ChatRequests
                .Include(x => x.FromEmployee)
                .Where(x => x.ToEmployeeId == toEmployeeId && x.Status == "Pending")
                .ToListAsync();
        }
        public async Task<int> CountUnreadMessagesAsync(int conversationId, int userId)
        {
            return await _context.Messages.CountAsync(m =>
                m.ConversationId == conversationId &&
                m.SenderId != userId &&
                !m.IsRead
            );
        }

        public async Task MarkMessagesAsReadAsync(int conversationId, int userId)
        {
            var msgs = await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var m in msgs)
                m.IsRead = true;

            await _context.SaveChangesAsync();
        }

    }
}
