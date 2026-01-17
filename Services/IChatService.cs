using HRMSystem.DTOs;

namespace HRMSystem.Services
{
    public interface IChatService
    {
        Task<ChatPermissionResult> CanStartChatAsync(int fromId, int toId);
        Task<ChatStartResultDto> StartChatAsync(int fromId, int toId);
        Task<List<ChatUserDto>> GetAvailableUsersAsync(int currentUserId);
        Task<bool> ApproveRequestAsync(int requestId, int approverId);
        Task<List<ChatRequestDto>> GetPendingRequestsForMeAsync(int myId);

    }
}
