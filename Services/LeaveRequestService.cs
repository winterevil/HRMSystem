using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IMapper _mapper;
        public LeaveRequestService(ILeaveRequestRepository repo, IMapper mapper, IEmployeeRepository employeeRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _employeeRepo = employeeRepo;
        }

        public async Task ApproveAsync(int id, LeaveStatus status, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var approvedById = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!currentRole.Contains("HR") && !currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to approve overtime requests.");
            }

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("Leave request not found.");
            }
            if (entity.Status == LeaveStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled leave requests cannot be approved or rejected.");
            }
            if (entity.Status != LeaveStatus.Pending && currentRole.Contains("Manager"))
            {
                throw new InvalidOperationException("Leave request is not in a state that can be updated.");
            }

            var approver = await _employeeRepo.GetByIdAsync(approvedById);
            var requester = await _employeeRepo.GetByIdAsync(entity.EmployeeId);
            if (approver == null || requester == null)
            {
                throw new InvalidOperationException("Approver or requester not found.");
            }
            if (currentRole.Contains("HR"))
            {
                if (entity.ApprovedById != null)
                {
                    var approverUser = await _employeeRepo.GetByIdAsync(entity.ApprovedById.Value);
                    if (approverUser != null && approverUser.EmployeeRoles.Any(er => er.Roles.RoleName == "HR"))
                    {
                        throw new InvalidOperationException("Leave request has already been checked by HR.");
                    }
                }
            }

            if (currentRole.Contains("Manager"))
            {
                if (approver.Id == requester.Id)
                {
                    throw new InvalidOperationException("Managers cannot approve their own leave requests.");
                }
                if (requester.EmployeeRoles != null &&
                    requester.EmployeeRoles.Any(er => er.Roles != null && er.Roles.RoleName == "HR"))
                {
                    throw new UnauthorizedAccessException("You cannot approve leave requests from HR employees.");
                }

                if (approver.DepartmentId != requester.DepartmentId)
                {
                    throw new UnauthorizedAccessException("You can only approve leave requests from your own department.");
                }
                entity.ApprovedById = approver.Id;
                entity.Status = status;
            }

            if (currentRole.Contains("HR"))
            {
                if (status == LeaveStatus.Pending)
                {
                    throw new InvalidOperationException("HR cannot set the status to Pending.");
                }
                entity.ApprovedById = approver.Id;
                entity.Status = status;
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task CreateAsync(LeaveRequestDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (currentRole.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Admins cannot create leave requests.");
            }
            var entity = _mapper.Map<LeaveRequest>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create leave request.");
            }
            if (entity.StartTime >= entity.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            if (entity.StartTime < DateTime.Now || entity.EndTime < DateTime.Now)
            {
                throw new InvalidOperationException("Leave request times must be in the future.");
            }
            int employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            entity.Employees = employee;
            entity.Status = LeaveStatus.Pending;
            entity.ApprovedById = null;
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var employeeId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var requests = await _repo.GetAllAsync();
            if (currentRole.Contains("HR"))
            {
                var r = requests.Select(r => _mapper.Map<LeaveRequestDto>(r));
                if (r == null || !r.Any())
                {
                    throw new InvalidOperationException("No leave requests found.");
                }
                return r;
            }
            else if (currentRole.Contains("Manager"))
            {
                var employee = await _employeeRepo.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    throw new InvalidOperationException("Employee not found.");
                }
                var departmentId = employee.DepartmentId;
                var r = requests
                    .Where(r => r.Employees.DepartmentId == departmentId)
                    .Select(r => _mapper.Map<LeaveRequestDto>(r));
                if (r == null || !r.Any())
                {
                    throw new InvalidOperationException("No leave requests found for your department.");
                }
                return r;
            }
            else if (currentRole.Contains("Employee"))
            {
                var r = requests
                    .Where(r => r.EmployeeId == employeeId)
                    .Select(r => _mapper.Map<LeaveRequestDto>(r));
                if (r == null || !r.Any())
                {
                    throw new InvalidOperationException("No leave requests found for your account.");
                }
                return r;
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view leave requests.");
            }
        }

        public async Task<LeaveRequestDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var employeeId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var request = await _repo.GetByIdAsync(id);
            if (currentRole.Contains("HR"))
            {
            }
            else if (currentRole.Contains("Manager"))
            {
                var employee = await _employeeRepo.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    throw new InvalidOperationException("Employee not found.");
                }
                if (request?.Employees.DepartmentId != employee.DepartmentId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this leave request.");
                }
            }
            else if (currentRole.Contains("Employee"))
            {
                if (request?.EmployeeId != employeeId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this leave request.");
                }

            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view leave requests.");
            }

            if (request == null)
            {
                throw new InvalidOperationException("Overtime request not found.");
            }
            return _mapper.Map<LeaveRequestDto>(request);
        }

        public async Task UpdateAsync(LeaveRequestDto dto, ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null)
            {
                throw new InvalidOperationException("Leave request not found.");
            }

            if (entity.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this leave request.");
            }

            if (entity.Status != LeaveStatus.Pending)
            {
                throw new InvalidOperationException("Only pending leave requests can be updated.");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }
            if (dto.StartTime < DateTime.Now || dto.EndTime < DateTime.Now)
            {
                throw new InvalidOperationException("Leave request times must be in the future.");
            }
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.Reason = dto.Reason;
            if (dto.Status != LeaveStatus.Cancelled && dto.Status != LeaveStatus.Pending)
            {
                throw new InvalidOperationException("You can only change the status to Cancelled.");
            }

            if (dto.Status == LeaveStatus.Cancelled)
            {
                entity.Status = LeaveStatus.Cancelled;
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
