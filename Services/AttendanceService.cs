using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepo;
        public AttendanceService(IAttendanceRepository repo, IMapper mapper, IEmployeeRepository employeeRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _employeeRepo = employeeRepo;
        }

        public async Task AutoCheckoutPendingAsync()
        {
            var pendingAttendances = await _repo.GetPendingCheckoutsBeforeAsync(DateTime.Today);

            if (pendingAttendances == null || !pendingAttendances.Any())
                return;

            foreach (var a in pendingAttendances)
            {
                a.CheckoutTime = a.CheckinDate.AddDays(1).AddSeconds(-1);
                _repo.Update(a);
            }

            await _repo.SaveChangesAsync();
        }

        public async Task CheckinAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (currentRole.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Admins cannot check in or out.");
            }

            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }
            var today = DateTime.Today;
            var existingAttendance = await _repo.GetByDateAsync(employeeId, today.Date);
            if (existingAttendance != null)
            {
                throw new InvalidOperationException("You have already checked in today.");
            }
            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                CheckinDate = today,
                CheckinTime = DateTime.UtcNow.AddHours(7),
                Employees = employee
            };
            await _repo.AddAsync(attendance);
            await _repo.SaveChangesAsync();
        }

        public async Task CheckoutAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (currentRole.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Admins cannot check in or out.");
            }
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }
            var today = DateTime.Today;
            var existingAttendance = await _repo.GetByDateAsync(employeeId, today.Date);
            if (existingAttendance == null || existingAttendance.CheckoutTime != null)
            {
                throw new InvalidOperationException("No check-in found or already checked out.");
            }

            existingAttendance.CheckoutTime = DateTime.UtcNow.AddHours(7);
            _repo.Update(existingAttendance);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<AttendanceDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var employee = await _employeeRepo.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }
            var attendances = await _repo.GetAllWithEmployeesAsync();

            if (currentRole.Contains("HR"))
            {
                var a = attendances.Select(a => _mapper.Map<AttendanceDto>(a));
                if (a == null || !a.Any())
                {
                    throw new InvalidOperationException("No attendance records found.");
                }
                return a;
            }
            else if (currentRole.Contains("Manager"))
            {
                var manager = await _employeeRepo.GetByIdAsync(employeeId);

                if (manager == null)
                {
                    throw new InvalidOperationException("Manager not found.");
                }
                var a = attendances.Where(a => a.Employees.DepartmentId == manager.DepartmentId)
                                  .Select(a => _mapper.Map<AttendanceDto>(a));
                if (a == null || !a.Any())
                {
                    throw new InvalidOperationException("No attendance records found for this department.");
                }
                return a;
            }
            else if (currentRole.Contains("Employee"))
            {
                var a = attendances.Where(a => a.EmployeeId == employeeId)
                                  .Select(a => _mapper.Map<AttendanceDto>(a));
                if (a == null || !a.Any())
                {
                    throw new InvalidOperationException("No attendance records found for this employee.");
                }
                return a;
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view attendance records.");
            }
        }
    }
}
