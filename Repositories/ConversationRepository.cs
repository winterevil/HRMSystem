using HRMSystem.Data;
using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _context;

        public ConversationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetDirectAsync(int emp1, int emp2)
        {
            return await _context.Conversations
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c =>
                    c.Members.Any(m => m.EmployeeId == emp1) &&
                    c.Members.Any(m => m.EmployeeId == emp2));
        }

        public async Task<Conversation> CreateDirectAsync(int emp1, int emp2, int createdBy)
        {
            var conversation = new Conversation
            {
                Type = "Direct",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                Members = new List<ConversationMember>
                {
                    new ConversationMember { EmployeeId = emp1 },
                    new ConversationMember { EmployeeId = emp2 }
                }
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }
    }
}
