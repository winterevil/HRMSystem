using System.Security.Claims;
using AutoMapper;
using HRMSystem.Data;
using HRMSystem.DTOs;
using HRMSystem.Repositories;
using HRMSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        public EmployeeService(IEmployeeRepository repo, IMapper mapper, AppDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }
        public async Task<IEnumerable<EmployeeDto>> GetAllAsync(EmployeeDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            IEnumerable<Employee> employees;
            if (currentRole.Contains("Admin") || currentRole.Contains("HR"))
            {
                employees = await _repo.GetAllAsync();
            }
            else if (currentRole.Contains("Manager"))
            {
                var managerId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var manager = await _context.Employees.FindAsync(managerId);
                var departmentId = manager?.DepartmentId;
                employees = (await _repo.GetAllAsync())
                    .Where(e => e.DepartmentId == departmentId);
                if (!employees.Any())
                {
                    throw new InvalidOperationException("No employees found in this department.");
                }
            }
            else
            {
                employees = Enumerable.Empty<Employee>();
                throw new UnauthorizedAccessException("You do not have permission to view this employee.");
            }

            if (employees == null || !employees.Any())
            {
                throw new InvalidOperationException("No employees found.");
            }

            return employees.Select(e => _mapper.Map<EmployeeDto>(e));
        }
        public async Task<EmployeeDto?> GetByIdAsync(int id, EmployeeDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            Employee employee;
            if (currentRole.Contains("Admin") || currentRole.Contains("HR"))
            {
                employee = await _repo.GetByIdAsync(id);
            }
            else if (currentRole.Contains("Manager"))
            {
                employee = await _repo.GetByIdAsync(id);
                if (employee.DepartmentId != dto.DepartmentId)
                {
                    employee = null;
                    throw new UnauthorizedAccessException("Managers can only view employees in their own department.");
                }
            }
            else
            {
                employee = await _repo.GetByIdAsync(id);
                if (employee.Id != int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"))
                {
                    employee = null;
                }
                throw new UnauthorizedAccessException("You do not have permission to view this employee.");
            }
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
                return null;
            }

            return _mapper.Map<EmployeeDto>(employee);
        }
        public async Task CreateAsync(EmployeeCreateDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (dto.Role == "Admin")
            {
                throw new UnauthorizedAccessException("Admin account must be created by the system.");
            }

            if (currentRole.Contains("Admin"))
            {
                if (dto.Role != "HR")
                {
                    if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                    {
                        throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                    }
                }
                else
                {
                    dto.DepartmentId = null;
                    dto.EmployeeTypeId = null;
                }
            }
            else if (currentRole.Contains("HR") && (dto.Role == "Manager" || dto.Role == "Employee"))
            {
                if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                {
                    throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                }
            }
            else if (currentRole.Contains("Manager") && (dto.Role == "Employee"))
            {
                var creatorId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                var manager = await _context.Employees.FindAsync(creatorId);
                if (manager == null || manager.DepartmentId != dto.DepartmentId)
                {
                    throw new UnauthorizedAccessException("Manager can only add employees in their own department.");
                }
                if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                {
                    throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to create this employee.");
            }
            var employees = await _repo.GetAllAsync();
            if (employees.Any(e => e.Email == dto.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }
            var employee = _mapper.Map<Employee>(dto);
            employee.HashPassword = new PasswordHasher<Employee>().HashPassword(employee, dto.Password);
            employee.CreatedAt = DateTime.UtcNow.AddHours(7);

            await _repo.AddAsync(employee);
            await _repo.SaveChangesAsync();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role != null)
            {
                await _context.EmployeeRoles.AddAsync(new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = role.Id
                });
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateAsync(EmployeeUpdateDto dto, ClaimsPrincipal user)
        {
            var employee = await _repo.GetByIdAsync(dto.Id);
            if (employee == null) return;

            var currentRoles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var role = await _context.EmployeeRoles.Include(r => r.Roles)
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id);
            var targetRole = role?.Roles?.RoleName ?? "";

            if (targetRole == "Admin")
                throw new UnauthorizedAccessException("You are not allowed to update the Admin account.");

            if (currentRoles.Contains("Admin"))
            {
                if (dto.Role != "HR")
                {
                    if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                    {
                        throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                    }
                }
                else
                {
                    dto.DepartmentId = null;
                    dto.EmployeeTypeId = null;
                }
            }
            else if (currentRoles.Contains("HR") && (targetRole == "Manager" || targetRole == "Employee"))
            {
                if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                {
                    throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                }
            }
            else if (currentRoles.Contains("Manager") && targetRole == "Employee")
            {
                var currentUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                var manager = await _context.Employees.FindAsync(currentUserId);
                if (manager == null || manager.DepartmentId != employee.DepartmentId)
                    throw new UnauthorizedAccessException("Managers can only update employees in their own department.");
                if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                {
                    throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                }
            }
            else throw new UnauthorizedAccessException("You are not allowed to update this employee.");
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var currentUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (currentUserId != dto.Id)
                {
                    throw new UnauthorizedAccessException("You can only change your own password.");
                }
                employee.HashPassword = new PasswordHasher<Employee>().HashPassword(employee, dto.Password);
            }
            _mapper.Map(dto, employee);
            _repo.Update(employee);
            await _repo.SaveChangesAsync();

            if (!string.IsNullOrEmpty(dto.Role))
            {
                var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
                if (newRole != null && role != null && role.RoleId != newRole.Id)
                {
                    role.RoleId = newRole.Id;
                    await _context.SaveChangesAsync();
                }

            }
        }

        public async Task DeleteAsync(int id, EmployeeDto dto, ClaimsPrincipal user)
        {
            var currentRoles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            Employee employee;
            if (currentRoles.Contains("Admin") || currentRoles.Contains("HR"))
            {
                employee = await _repo.GetByIdAsync(id);
            }
            else
            {
                employee = null;
                throw new UnauthorizedAccessException("You do not have permission to delete this employee.");
            }
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }


            var deleted = new DeletedEmployee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                HashPassword = employee.HashPassword,
                Gender = employee.Gender,
                DOB = employee.DOB,
                Phone = employee.Phone,
                Address = employee.Address,
                CreatedAt = employee.CreatedAt,
                Status = (int)employee.Status,
                DepartmentId = (int)employee.DepartmentId,
                EmployeeTypeId = (int)employee.EmployeeTypeId,
                DeletedAt = DateTime.UtcNow.AddHours(7),
                DeletedById = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            await _repo.DeleteWithArchiveAsync(employee, deleted);
            await _repo.SaveChangesAsync();
        }
    }
}
