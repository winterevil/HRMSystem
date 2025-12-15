using System.Data;
using System.Security.Claims;
using AutoMapper;


using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class EmployeeTypeService:IEmployeeTypeService
    {
        private readonly IEmployeeTypeRepository _repo;
        private readonly IMapper _mapper;
        public EmployeeTypeService(IEmployeeTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<IEnumerable<EmployeeTypeDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            //if (!currentRole.Contains("HR") && !currentRole.Contains("Admin"))
            //{
            //   throw new UnauthorizedAccessException("You do not have permission to view employee types.");
            //}

            var types = await _repo.GetAllAsync();

            if (types == null || !types.Any())
            {
                throw new InvalidOperationException("No employee type found.");
            }

            return _mapper.Map<IEnumerable<EmployeeTypeDto>>(types);
        }

        public async Task<EmployeeTypeDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to view employee types.");
            }

            var entity = await _repo.GetByIdAsync(id);

            if (entity == null)
            {
                throw new InvalidOperationException("Employee type not found.");
            }

            return _mapper.Map<EmployeeTypeDto>(entity);
        }
        public async Task CreateAsync(EmployeeTypeDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create employee types.");
            }
            if (dto.TypeName.Trim().Equals("System", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("You cannot create or modify the System employee type.");

            var types = await _repo.GetAllAsync();
            var entity = _mapper.Map<EmployeeType>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Employee type not found.");
            }
            if (types.Any(t => t.TypeName == entity.TypeName))
            {
                throw new InvalidOperationException("Employee type already exists.");
            }

            entity.CreatedAt = DateTime.UtcNow.AddHours(7);
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }
        public async Task UpdateAsync(EmployeeTypeDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update employee types.");
            }

            var entity = _mapper.Map<EmployeeType>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Employee type not found.");
            }
            if (entity.TypeName == "System")
                throw new InvalidOperationException("The System employee type cannot be edited or renamed.");
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
                throw new UnauthorizedAccessException("You do not have permission to delete employee types.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException("Employee type not found.");

            if (entity.TypeName.Equals("System", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The System employee type cannot be deleted.");

            var count = await _repo.CountEmployeesByTypeAsync(id);
            if (count > 0)
                throw new InvalidOperationException("Cannot delete an employee type that is currently assigned to employees.");

            var deleted = new DeletedEmployeeType
            {
                Id = entity.Id,
                TypeName = entity.TypeName,
                CreatedAt = entity.CreatedAt,
                DeletedAt = DateTime.UtcNow.AddHours(7),
                DeletedById = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            await _repo.DeleteWithArchiveAsync(entity, deleted);
            await _repo.SaveChangesAsync();
        }
    }
}
