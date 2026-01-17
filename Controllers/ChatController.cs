using System.Security.Claims;
using HRMSystem.Data;
using HRMSystem.DTOs;
using HRMSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly AppDbContext _context;
        public ChatController(IChatService chatService, AppDbContext context)
        {
            _chatService = chatService;
            _context = context;
        }
        [HttpPost("can-start")]
        public async Task<IActionResult> CanStart(ChatStartDto dto)
        {
            var result = await _chatService.CanStartChatAsync(dto.FromId, dto.ToId);
            return Ok(result);
        }
        [HttpPost("start")]
        public async Task<IActionResult> StartChat(ChatStartDto dto)
        {
            var result = await _chatService.StartChatAsync(dto.FromId, dto.ToId);
            return Ok(result);
        }
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var myId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != myId &&
                    !m.IsRead
                )
                .ExecuteUpdateAsync(m =>
                    m.SetProperty(x => x.IsRead, true)
                );

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("available-users")]
        public async Task<IActionResult> GetAvailableUsers()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (userIdClaim == null)
                return Unauthorized("UserId claim not found");

            var userId = int.Parse(userIdClaim.Value);

            var users = await _chatService.GetAvailableUsersAsync(userId);
            return Ok(users);
        }
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var approverId = int.Parse(
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value
            );

            var ok = await _chatService.ApproveRequestAsync(id, approverId);
            if (!ok) return BadRequest();

            return Ok();
        }
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var myId = int.Parse(
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value
            );

            var data = await _chatService.GetPendingRequestsForMeAsync(myId);
            return Ok(data);
        }
        [HttpPost("{conversationId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            var myId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != myId &&
                    !m.IsRead
                )
                .ExecuteUpdateAsync(m =>
                    m.SetProperty(x => x.IsRead, true)
                );

            return Ok();
        }

    }
}
