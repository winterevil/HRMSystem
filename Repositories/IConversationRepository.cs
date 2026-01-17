using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetDirectAsync(int emp1, int emp2);
        Task<Conversation> CreateDirectAsync(int emp1, int emp2, int createdBy);
    }
}
