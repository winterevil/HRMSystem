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
                throw new InvalidOperationException("Overtime request not found.");
            }
            if (entity.Status != LeaveStatus.Pending && currentRole.Contains("Manager"))
            {
                throw new InvalidOperationException("Overtime request is not in a state that can be updated.");
            }

            var approver = await _employeeRepo.GetByIdAsync(approvedById);
            var requester = await _employeeRepo.GetByIdAsync(entity.EmployeeId);
            if (approver == null || requester == null)
            {
                throw new InvalidOperationException("Approver or requester not found.");
            }
            if (approver.EmployeeRoles.Any(er => er.Roles != null && er.Roles.RoleName == "HR"))
            {
                throw new InvalidOperationException("Overtime request has already been checked by HR.");
            }
            if (currentRole.Contains("Manager"))
            {
                if (approver.Id == requester.Id)
                {
                    throw new InvalidOperationException("Managers cannot approve their own overtime requests.");
                }
                if (requester.EmployeeRoles != null &&
                    requester.EmployeeRoles.Any(er => er.Roles != null && er.Roles.RoleName == "HR"))
                {
                    throw new UnauthorizedAccessException("You cannot approve overtime requests from HR employees.");
                }

                if (approver.DepartmentId != requester.DepartmentId)
                {
                    throw new UnauthorizedAccessException("You can only approve overtime requests from your own department.");
                }
                entity.ApprovedBy = approver;
                entity.Status = status;
            }

            if (currentRole.Contains("HR"))
            {
                if (status == LeaveStatus.Pending)
                {
                    throw new InvalidOperationException("HR cannot set the status to Pending.");
                }
                entity.ApprovedBy = approver;
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
                throw new UnauthorizedAccessException("Admins cannot create overtime requests.");
            }
            var entity = _mapper.Map<LeaveRequest>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create overtime request.");
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
                    throw new InvalidOperationException("No overtime requests found.");
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
                    throw new InvalidOperationException("No overtime requests found for your department.");
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
                    throw new InvalidOperationException("No overtime requests found for your account.");
                }
                return r;
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view overtime requests.");
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
                    throw new UnauthorizedAccessException("You do not have permission to view this overtime request.");
                }
            }
            else if (currentRole.Contains("Employee"))
            {
                if (request?.EmployeeId != employeeId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this overtime request.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view overtime requests.");
            }

            if (request == null)
            {
                throw new InvalidOperationException("Overtime request not found.");
            }
            return _mapper.Map<LeaveRequestDto>(request);
        }

        public Task UpdateAsync(LeaveRequestDto dto, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }
    }
}
