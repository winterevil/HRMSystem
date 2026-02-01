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
        private readonly DeletedDbContext _deletedContext;
        public EmployeeService(IEmployeeRepository repo, IMapper mapper, AppDbContext context, DeletedDbContext deletedContext)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
            _deletedContext = deletedContext;
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
                var currentId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var employee = await _repo.GetByIdAsync(currentId);

                if (employee == null)
                {
                    throw new InvalidOperationException("Employee not found.");
                }
                employees = new List<Employee> { employee };
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
            }

            return _mapper.Map<EmployeeDto>(employee);
        }
        public async Task CreateAsync(EmployeeCreateDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var adminDept = await _context.Departments
                .FirstAsync(d => d.DepartmentName == "Administration");

            if (dto.RoleId == 1)
            {
                throw new UnauthorizedAccessException("Admin account must be created by the system.");
            }
            if (dto.EmployeeTypeId == 1)
            {
                throw new UnauthorizedAccessException("System employee type cannot be assigned manually.");
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
                    if (dto.DepartmentId != adminDept.Id)
                    {
                        throw new InvalidOperationException("HR must belong to the Administration.");
                    }
                    if (dto.EmployeeTypeId == null)
                    {
                        throw new InvalidOperationException("EmployeeTypeId must be provided.");
                    }
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
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (!Enum.TryParse<EmployeeStatus>(dto.Status, out var statusEnum))
                    throw new InvalidOperationException("Invalid employee status.");

                employee.Status = statusEnum;
            }
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

            var adminDept = await _context.Departments
                .FirstAsync(d => d.DepartmentName == "Administration");

            var currentRoles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var currentUserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var role = await _context.EmployeeRoles.Include(r => r.Roles)
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id);
            var targetRole = role?.Roles?.RoleName ?? "";

            if (targetRole == "Admin")
                throw new UnauthorizedAccessException("You are not allowed to update the Admin account.");

            var isSelfUpdate = currentUserId == dto.Id;

            if (isSelfUpdate)
            {
                employee.FullName = dto.FullName;
                employee.Phone = dto.Phone;
                employee.Address = dto.Address;
                employee.DOB = dto.DOB;
                employee.Gender = dto.Gender;

                if (!string.IsNullOrEmpty(dto.NewPassword))
                {
                    if (string.IsNullOrEmpty(dto.OldPassword))
                        throw new InvalidOperationException("Old password is required.");

                    var result = new PasswordHasher<Employee>()
                        .VerifyHashedPassword(employee, employee.HashPassword, dto.OldPassword);

                    if (result == PasswordVerificationResult.Failed)
                        throw new InvalidOperationException("Old password is incorrect.");

                    // Update to new password
                    employee.HashPassword = new PasswordHasher<Employee>()
                        .HashPassword(employee, dto.NewPassword);
                }

                _repo.Update(employee);
                await _repo.SaveChangesAsync();
                return;
            }


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
                    if (dto.DepartmentId != adminDept.Id)
                    {
                        throw new InvalidOperationException("HR must belong to the Administration.");
                    }
                    if (dto.EmployeeTypeId == null)
                    {
                        throw new InvalidOperationException("EmployeeTypeId must be provided.");
                    }
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
                var manager = await _context.Employees.FindAsync(currentUserId);

                if (manager == null || manager.DepartmentId != employee.DepartmentId)
                    throw new UnauthorizedAccessException("Managers can only update employees in their own department.");

                if (dto.DepartmentId == null || dto.EmployeeTypeId == null)
                {
                    throw new InvalidOperationException("DepartmentId and EmployeeTypeId must be provided.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You are not allowed to update this employee.");
            }

            _mapper.Map(dto, employee);
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (!Enum.TryParse<EmployeeStatus>(dto.Status, out var statusEnum))
                    throw new InvalidOperationException("Invalid employee status.");

                employee.Status = statusEnum;
            }
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

            if (!currentRoles.Contains("Admin") && !currentRoles.Contains("HR"))
                throw new UnauthorizedAccessException("You do not have permission to delete this employee.");

            var employee = await _repo.GetByIdAsync(id);
            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.EmployeeRoles != null &&
                employee.EmployeeRoles.Any(er => er.Roles.RoleName == "Admin"))
                throw new UnauthorizedAccessException("You are not allowed to delete the Admin account.");

            var role = employee.EmployeeRoles?.FirstOrDefault();
            if (role == null)
                throw new InvalidOperationException("Employee has no role.");

            bool hasRelation =
                await _context.Attendance.AnyAsync(a => a.EmployeeId == id) ||
                await _context.LeaveRequests.AnyAsync(l => l.EmployeeId == id) ||
                await _context.OvertimeRequests.AnyAsync(o => o.EmployeeId == id);

            if (hasRelation)
                throw new InvalidOperationException(
                    "Cannot delete employee because related records still exist."
                );

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
                DepartmentId = employee.DepartmentId ?? 0,
                EmployeeTypeId = employee.EmployeeTypeId ?? 0,
                RoleId = role.RoleId,
                DeletedAt = DateTime.UtcNow.AddHours(7),
                DeletedById = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!)
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                await _deletedContext.DeletedEmployees.AddAsync(deleted);
                await _deletedContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "Delete failed. Employee still has related data."
                );
            }
        }

        public async Task<IEnumerable<EmployeeWithDeletedDto>> GetAllIncludeDeletedAsync(ClaimsPrincipal user)
        {
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            IEnumerable<Employee> activeEmployees;

            if (roles.Contains("Admin") || roles.Contains("HR"))
            {
                activeEmployees = await _repo.GetAllAsync();
            }
            else if (roles.Contains("Manager"))
            {
                var manager = await _context.Employees.FindAsync(userId);
                activeEmployees = (await _repo.GetAllAsync())
                    .Where(e => e.DepartmentId == manager!.DepartmentId);
            }
            else
            {
                var employee = await _repo.GetByIdAsync(userId);
                activeEmployees = employee != null
                    ? new List<Employee> { employee }
                    : Enumerable.Empty<Employee>();
            }

            var result = new List<EmployeeWithDeletedDto>();

            result.AddRange(activeEmployees.Select(e =>
            {
                var dto = _mapper.Map<EmployeeDto>(e);

                return new EmployeeWithDeletedDto
                {
                    Id = dto.Id,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    DOB = dto.DOB,
                    Gender = dto.Gender,
                    DepartmentId = dto.DepartmentId,
                    DepartmentName = dto.DepartmentName,
                    EmployeeTypeId = dto.EmployeeTypeId,
                    TypeName = dto.TypeName,
                    RoleId = dto.RoleId,
                    RoleName = dto.RoleName,
                    Status = dto.Status,
                    CreatedAt = dto.CreatedAt,
                    IsDeleted = false
                };
            }));

            if (roles.Contains("Admin") || roles.Contains("HR"))
            {
                var deletedEmployees = await _repo.GetAllDeletedAsync();

                result.AddRange(deletedEmployees.Select(d => new EmployeeWithDeletedDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Email = d.Email,
                    Phone = d.Phone,
                    Address = d.Address,
                    DOB = d.DOB,
                    Gender = d.Gender,

                    DepartmentId = d.DepartmentId,
                    DepartmentName = "(Deleted)",

                    EmployeeTypeId = d.EmployeeTypeId,
                    TypeName = "(Deleted)",

                    RoleId = d.RoleId,
                    RoleName = "(Deleted)",

                    Status = ((EmployeeStatus)d.Status).ToString(),
                    CreatedAt = d.CreatedAt,

                    IsDeleted = true,
                    DeletedAt = d.DeletedAt,
                    DeletedById = d.DeletedById
                }));
            }

            return result;
        }


        public async Task RestoreAsync(int employeeId, ClaimsPrincipal user)
        {
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (!roles.Contains("Admin") && !roles.Contains("HR"))
                throw new UnauthorizedAccessException("You do not have permission to restore.");

            var deleted = await _repo.GetDeletedByIdAsync(employeeId);
            if (deleted == null)
                throw new InvalidOperationException("Deleted employee not found.");

            if (_context.Employees.Any(e => e.Email == deleted.Email))
                throw new InvalidOperationException("Email already exists.");

            var employee = new Employee
            {
                FullName = deleted.FullName,
                Email = deleted.Email,
                HashPassword = deleted.HashPassword,
                Gender = deleted.Gender,
                DOB = deleted.DOB,
                Phone = deleted.Phone,
                Address = deleted.Address,
                CreatedAt = deleted.CreatedAt,
                Status = (EmployeeStatus)deleted.Status,
                DepartmentId = deleted.DepartmentId,
                EmployeeTypeId = deleted.EmployeeTypeId
            };

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync(); 

            _context.EmployeeRoles.Add(new EmployeeRole
            {
                EmployeeId = employee.Id, 
                RoleId = deleted.RoleId
            });

            _deletedContext.Remove(deleted);

            await _context.SaveChangesAsync();
            await _deletedContext.SaveChangesAsync();
        }

    }
}
