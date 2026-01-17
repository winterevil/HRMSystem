using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class ChatService : IChatService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IChatRepository _chatRepo;
        private readonly IConversationRepository _conversationRepo;

        public ChatService(
            IEmployeeRepository employeeRepo,
            IChatRepository chatRepo,
            IConversationRepository conversationRepo)
        {
            _employeeRepo = employeeRepo;
            _chatRepo = chatRepo;
            _conversationRepo = conversationRepo;
        }

        private string GetPrimaryRole(Employee e)
        {
            if (e.EmployeeRoles.Any(r => r.Roles.RoleName == "HR"))
                return "HR";

            if (e.EmployeeRoles.Any(r => r.Roles.RoleName == "Manager"))
                return "Manager";

            return "Employee";
        }

        private async Task<int> GetUnreadCountAsync(int myId, int otherId)
        {
            var convo = await _conversationRepo.GetDirectAsync(myId, otherId);
            if (convo == null) return 0;

            return await _chatRepo.CountUnreadMessagesAsync(convo.Id, myId);
        }

        public async Task<ChatPermissionResult> CanStartChatAsync(int fromId, int toId)
        {
            if (await _conversationRepo.GetDirectAsync(fromId, toId) != null)
                return new() { CanChat = true };

            var from = await _employeeRepo.GetByIdAsync(fromId);
            var to = await _employeeRepo.GetByIdAsync(toId);
            if (from == null || to == null)
                return new() { CanChat = false };

            var fromRole = GetPrimaryRole(from);
            var toRole = GetPrimaryRole(to);

            if (fromRole == "HR")
                return new() { CanChat = true };

            if (fromRole == "Manager" && toRole == "Employee"
                && from.DepartmentId == to.DepartmentId)
                return new() { CanChat = true };

            return new() { NeedApproval = true };
        }

        public async Task<ChatStartResultDto> StartChatAsync(int fromId, int toId)
        {
            var existing = await _conversationRepo.GetDirectAsync(fromId, toId);
            if (existing != null)
            {
                return new ChatStartResultDto
                {
                    CanChat = true,
                    ConversationId = existing.Id
                };
            }

            var permission = await CanStartChatAsync(fromId, toId);

            if (permission.NeedApproval)
            {
                var req = await _chatRepo.GetPendingAsync(fromId, toId)
                          ?? await _chatRepo.CreateAsync(new ChatRequest
                          {
                              FromEmployeeId = fromId,
                              ToEmployeeId = toId,
                              Status = "Pending"
                          });

                return new ChatStartResultDto
                {
                    NeedApproval = true,
                    RequestId = req.Id
                };
            }

            var convo = await _conversationRepo.CreateDirectAsync(fromId, toId, fromId);

            return new ChatStartResultDto
            {
                CanChat = true,
                ConversationId = convo.Id
            };
        }

        public async Task<bool> ApproveRequestAsync(int requestId, int approverId)
        {
            var req = await _chatRepo.GetByIdAsync(requestId);
            if (req == null || req.Status != "Pending")
                return false;

            if (req.ToEmployeeId != approverId)
                return false;

            await _chatRepo.ApproveAsync(requestId, approverId);

            await _conversationRepo.CreateDirectAsync(
                req.FromEmployeeId,
                req.ToEmployeeId,
                approverId
            );

            return true;
        }

        public async Task<List<ChatUserDto>> GetAvailableUsersAsync(int currentUserId)
        {
            var result = new List<ChatUserDto>();

            var me = await _employeeRepo.GetByIdAsync(currentUserId);
            if (me == null) return result;

            var myRole = GetPrimaryRole(me);

            /* ===== EMPLOYEE ===== */
            if (myRole == "Employee")
            {
                if (me.DepartmentId.HasValue)
                {
                    var managers = await _employeeRepo.GetEmployeesByRoleAsync("Manager");
                    foreach (var m in managers.Where(x => x.DepartmentId == me.DepartmentId))
                    {
                        var hasConversation =
                            await _conversationRepo.GetDirectAsync(me.Id, m.Id) != null;

                        var pending =
                            await _chatRepo.GetPendingAsync(me.Id, m.Id);

                        result.Add(new ChatUserDto
                        {
                            Id = m.Id,
                            FullName = m.FullName,
                            Role = "Manager",

                            CanChatDirect = hasConversation,
                            NeedApproval = !hasConversation && pending == null,
                            IsPending = pending != null,

                            UnreadCount = await GetUnreadCountAsync(me.Id, m.Id)
                        });
                    }
                }

                var hrs = await _employeeRepo.GetEmployeesByRoleAsync("HR");
                foreach (var hr in hrs)
                {
                    var hasConversation =
                        await _conversationRepo.GetDirectAsync(me.Id, hr.Id) != null;

                    var pending =
                        await _chatRepo.GetPendingAsync(me.Id, hr.Id);

                    result.Add(new ChatUserDto
                    {
                        Id = hr.Id,
                        FullName = hr.FullName,
                        Role = "HR",

                        CanChatDirect = hasConversation,
                        NeedApproval = !hasConversation && pending == null,
                        IsPending = pending != null,

                        UnreadCount = await GetUnreadCountAsync(me.Id, hr.Id)
                    });
                }

                return result;
            }

            /* ===== MANAGER ===== */
            if (myRole == "Manager")
            {
                var employees = await _employeeRepo.GetEmployeesByRoleAsync("Employee");
                foreach (var e in employees.Where(x => x.DepartmentId == me.DepartmentId))
                {
                    result.Add(new ChatUserDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        Role = "Employee",
                        CanChatDirect = true,
                        UnreadCount = await GetUnreadCountAsync(me.Id, e.Id)
                    });
                }

                var hrs = await _employeeRepo.GetEmployeesByRoleAsync("HR");
                foreach (var hr in hrs)
                {
                    var hasConversation =
                        await _conversationRepo.GetDirectAsync(me.Id, hr.Id) != null;

                    var pending =
                        await _chatRepo.GetPendingAsync(me.Id, hr.Id);

                    result.Add(new ChatUserDto
                    {
                        Id = hr.Id,
                        FullName = hr.FullName,
                        Role = "HR",

                        CanChatDirect = hasConversation,
                        NeedApproval = !hasConversation && pending == null,
                        IsPending = pending != null,

                        UnreadCount = await GetUnreadCountAsync(me.Id, hr.Id)
                    });
                }

                return result;
            }

            /* ===== HR ===== */
            if (myRole == "HR")
            {
                var all = await _employeeRepo.GetAllAsync();
                foreach (var e in all.Where(x => x.Id != currentUserId))
                {
                    result.Add(new ChatUserDto
                    {
                        Id = e.Id,
                        FullName = e.FullName,
                        Role = GetPrimaryRole(e),
                        CanChatDirect = true,
                        UnreadCount = await GetUnreadCountAsync(me.Id, e.Id)
                    });
                }
            }

            return result;
        }

        public async Task<List<ChatRequestDto>> GetPendingRequestsForMeAsync(int myId)
        {
            var reqs = await _chatRepo.GetPendingForReceiverAsync(myId);

            return reqs.Select(r => new ChatRequestDto
            {
                RequestId = r.Id,
                FromEmployeeId = r.FromEmployeeId,
                FromEmployeeName = r.FromEmployee.FullName
            }).ToList();
        }
    }
}
