using System.Security.Claims;
using AutoMapper;


using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class DepartmentService:IDepartmentService
    {
        private readonly IDepartmentRepository _repo;
        private readonly IMapper _mapper;
        public DepartmentService(IDepartmentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<IEnumerable<DepartmentDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            //if (!currentRole.Contains("HR") && !currentRole.Contains("Admin"))
            //{
            //   throw new UnauthorizedAccessException("You do not have permission to view employee types.");
            //}

            var departments = await _repo.GetAllAsync();

            if (departments == null || !departments.Any())
            {
                throw new InvalidOperationException("No department found.");
            }

            return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to view departments.");
            }

            var entity = await _repo.GetByIdAsync(id);

            if (entity == null)
            {
                throw new InvalidOperationException("Department not found.");
            }
            return _mapper.Map<DepartmentDto>(entity);
        }
        public async Task CreateAsync(DepartmentDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create departments.");
            }
            var departments = await _repo.GetAllAsync();
            var entity = _mapper.Map<Department>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Department not found.");
            }
            if (departments.Any(t => t.DepartmentName == entity.DepartmentName))
            {
                throw new InvalidOperationException("Department already exists.");
            }

            entity.CreatedAt = DateTime.UtcNow.AddHours(7);
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }
        public async Task UpdateAsync(DepartmentDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update departments.");
            }
            var entity = _mapper.Map<Department>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Department not found.");
            }

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to delete departments.");
            }
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("Department not found.");
            }

            var deleted = new DeletedDepartment
            {
                Id = entity.Id,
                DepartmentName = entity.DepartmentName,
                CreatedAt = entity.CreatedAt,
                DeletedAt = DateTime.UtcNow.AddHours(7),
                DeletedById = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            await _repo.DeleteWithArchiveAsync(entity, deleted);
            await _repo.SaveChangesAsync();
        }

    }
}
