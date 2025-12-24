using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class OvertimeRequestService : IOvertimeRequestService
    {
        private readonly IOvertimeRequestRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly IMapper _mapper;
        public OvertimeRequestService(IOvertimeRequestRepository repo, IMapper mapper, IEmployeeRepository employeeRepo, ILeaveRequestRepository leaveRequestRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _employeeRepo = employeeRepo;
            _leaveRequestRepo = leaveRequestRepo;
        }

        public async Task ApproveAsync(int id, OvertimeStatus status, ClaimsPrincipal user)
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
            if (entity.Status == OvertimeStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled overtime requests cannot be approved or rejected.");
            }

            if (entity.Status != OvertimeStatus.Pending && currentRole.Contains("Manager"))
            {
                throw new InvalidOperationException("Overtime request is not in a state that can be updated.");
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
                        throw new InvalidOperationException("Overtime request has already been checked by HR.");
                    }
                }
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
                entity.ApprovedById = approver.Id;
                entity.Status = status;
            }

            if (currentRole.Contains("HR"))
            {
                if (status == OvertimeStatus.Pending)
                {
                    throw new InvalidOperationException("HR cannot set the status to Pending.");
                }
                entity.ApprovedById = approver.Id;
                entity.Status = status;
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task CreateAsync(OvertimeRequestDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (currentRole.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Admins cannot create overtime requests.");
            }
            var entity = _mapper.Map<OvertimeRequest>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create overtime request.");
            }
            if (entity.StartTime >= entity.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }

            if (entity.StartTime < DateTime.Now || entity.EndTime < DateTime.Now)
            {
                throw new InvalidOperationException("Overtime request times must be in the future.");
            }
            int employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var approvedLeaves = await _leaveRequestRepo.GetApprovedLeavesByEmployeeIdAsync(employeeId);

            bool hasOverlapLeave = approvedLeaves.Any(l =>
                l.StartTime.Date <= dto.EndTime.Date &&
                l.EndTime.Date >= dto.StartTime.Date
            );

            if (hasOverlapLeave)
            {
                throw new InvalidOperationException(
                    "You cannot create an overtime request during an approved leave period."
                );
            }
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            entity.Employees = employee;
            entity.Status = OvertimeStatus.Pending;
            entity.ApprovedById = null;
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OvertimeRequestDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var employeeId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var requests = await _repo.GetAllAsync();
            if (currentRole.Contains("HR"))
            {
                var r = requests.Select(r => _mapper.Map<OvertimeRequestDto>(r));
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
                    .Select(r => _mapper.Map<OvertimeRequestDto>(r));
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
                    .Select(r => _mapper.Map<OvertimeRequestDto>(r));
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

        public async Task<OvertimeRequestDto?> GetByIdAsync(int id, ClaimsPrincipal user)
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
            return _mapper.Map<OvertimeRequestDto>(request);
        }

        public async Task UpdateAsync(OvertimeRequestDto dto, ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null)
            {
                throw new InvalidOperationException("Overtime request not found.");
            }

            if (entity.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this overtime request.");
            }

            if (entity.Status != OvertimeStatus.Pending)
            {
                throw new InvalidOperationException("Only pending overtime requests can be updated.");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }
            if (dto.StartTime < DateTime.Now || dto.EndTime < DateTime.Now)
            {
                throw new InvalidOperationException("Overtime request times must be in the future.");
            }
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.Reason = dto.Reason;
            if (dto.Status != OvertimeStatus.Cancelled && dto.Status != OvertimeStatus.Pending)
            {
                throw new InvalidOperationException("You can only change the status to Cancelled.");
            }

            if (dto.Status == OvertimeStatus.Cancelled)
            {
                entity.Status = OvertimeStatus.Cancelled;
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
    }
}
