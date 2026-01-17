using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IChatRepository
    {
        Task<ChatRequest?> GetPendingAsync(int fromId, int toId);
        Task<ChatRequest> CreateAsync(ChatRequest request);
        Task ApproveAsync(int requestId, int approverId);
        Task<ChatRequest?> GetByIdAsync(int id);
        Task<List<ChatRequest>> GetPendingForReceiverAsync(int toEmployeeId);
        Task<int> CountUnreadMessagesAsync(int conversationId, int userId);
        Task MarkMessagesAsReadAsync(int conversationId, int userId);
    }
}
