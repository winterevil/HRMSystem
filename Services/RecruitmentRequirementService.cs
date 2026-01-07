using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class RecruitmentRequirementService : IRecruitmentRequirementService
    {
        private readonly IRecruitmentRequirementRepository _repo;
        private readonly IMapper _mapper;
        private readonly IRecruimentPositionRepository _positionRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IJobPostRepository _jobPostRepo;
        private readonly INotificationService _notificationService;
        public RecruitmentRequirementService(IRecruitmentRequirementRepository repo, IMapper mapper, IRecruimentPositionRepository positionRepo, IEmployeeRepository employeeRepo, IJobPostRepository jobPostRepo, INotificationService notificationService)
        {
            _repo = repo;
            _mapper = mapper;
            _positionRepo = positionRepo;
            _employeeRepo = employeeRepo;
            _jobPostRepo = jobPostRepo;
            _notificationService = notificationService;
        }
        public async Task UpdateJobPostsStatusAsync(RecruitmentRequirement recruitment)
        {
            var jobPosts = await _jobPostRepo.GetByRequirementIdAsync(recruitment.Id);
            if (jobPosts == null || !jobPosts.Any()) return;

            JobPostStatus newStatus = recruitment.Status switch
            {
                RecruitmentStatus.Approved => JobPostStatus.Hiring,
                RecruitmentStatus.Completed => JobPostStatus.Closed,
                _ => JobPostStatus.Hiring
            };

            foreach (var jobPost in jobPosts)
            {
                jobPost.Status = newStatus;
                _jobPostRepo.Update(jobPost);
            }

            await _jobPostRepo.SaveChangesAsync();
        }
        public async Task ApproveAsync(int id, RecruitmentStatus status, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to approve recruitment requirements.");
            }

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }
            var validTransitions = new Dictionary<RecruitmentStatus, RecruitmentStatus[]>
            {
                { RecruitmentStatus.Pending, new[] { RecruitmentStatus.Approved, RecruitmentStatus.Rejected } },
                { RecruitmentStatus.Approved, new[] { RecruitmentStatus.Completed } }
            };

            if (!validTransitions.TryGetValue(entity.Status, out var allowedNext) || !allowedNext.Contains(status))
            {
                throw new InvalidOperationException(
                    $"Recruitment requirement cannot be changed from {entity.Status} to {status}."
                );
            }
            entity.Status = status;
            _repo.Update(entity);
            await _repo.SaveChangesAsync();
            await UpdateJobPostsStatusAsync(entity);
            var recipients = new List<int>
            {
                entity.EmployeeId 
            };

            var notiType = status == RecruitmentStatus.Approved
                ? NotificationType.RecruitmentApproved
                : NotificationType.RecruitmentRejected;

            await _notificationService.CreateAsync(
                notiType,
                $"Recruitment {status}",
                $"Your recruitment request for position '{entity.RecruitmentPositions.PositionName}' has been {status.ToString().ToLower()}.",
                recipients
            );

        }

        public async Task CreateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (!currentRole.Contains("Manager"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create recruitment requirements.");
            }
            var positionId = dto.PositionId;
            if (positionId <= 0)
            {
                throw new InvalidOperationException("Invalid position ID provided.");
            }
            var entity = _mapper.Map<RecruitmentRequirement>(dto);
            if (entity == null)
            {
                throw new InvalidOperationException("Failed to create recruitment requirement.");
            }
            var position = await _positionRepo.GetByIdAsync(positionId);
            if (position == null)
            {
                throw new InvalidOperationException("Position not found.");
            }
            var employee = await _employeeRepo.GetByIdAsync(int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }
            entity.CreatedAt = DateTime.UtcNow.AddHours(7);
            entity.Employees = employee;
            entity.RecruitmentPositions = position;
            entity.Status = RecruitmentStatus.Pending;
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RecruitmentRequirementDto>> GetAllAsync(ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var requirements = await _repo.GetAllAsync();
            if (currentRole.Contains("HR"))
            {
            }
            else if (currentRole.Contains("Manager"))
            {
                int currentEmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                requirements = requirements.Where(r => r.EmployeeId == currentEmployeeId);
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment requirements.");
            }

            if (requirements == null || !requirements.Any())
            {
                throw new InvalidOperationException("No recruitment requirements found.");
            }
            return _mapper.Map<IEnumerable<RecruitmentRequirementDto>>(requirements);
        }

        public async Task<RecruitmentRequirementDto?> GetByIdAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var requirement = await _repo.GetByIdAsync(id);
            if (currentRole.Contains("HR"))
            {
            }
            else if (currentRole.Contains("Manager"))
            {
                int currentEmployeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                if (requirement != null && requirement.Employees.Id != currentEmployeeId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this recruitment requirement.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You do not have permission to view recruitment requirements.");
            }
            if (requirement == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }
            return _mapper.Map<RecruitmentRequirementDto>(requirement);
        }

        //public async Task UpdateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user)
        //{
        //    var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        //    if (!currentRole.Contains("Manager"))
        //    {
        //        throw new UnauthorizedAccessException("You do not have permission to update recruitment requirements.");
        //    }
        //    var entity = await _repo.GetByIdAsync(dto.Id);
        //    if (entity == null)
        //    {
        //        throw new InvalidOperationException("Recruitment requirement not found.");
        //    }
        //    if (entity.Status != RecruitmentStatus.Pending)
        //    {
        //        throw new InvalidOperationException("Recruitment requirement is not in a state that can be updated.");
        //    }
        //    _mapper.Map(dto, entity);
        //    var position = await _positionRepo.GetByIdAsync(dto.PositionId);
        //    if (position == null)
        //    {
        //        throw new InvalidOperationException("Position not found.");
        //    }
        //    entity.RecruitmentPositions = position;
        //    var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
        //    if (employeeId == null)
        //    {
        //        throw new InvalidOperationException("Employee is required.");
        //    }
        //    var employee = await _employeeRepo.GetByIdAsync(employeeId);
        //    if (employee == null)
        //    {
        //        throw new InvalidOperationException("Employee not found.");
        //    }
        //    entity.EmployeeId = employeeId;
        //    entity.Employees = employee;
        //    entity.Status = RecruitmentStatus.Pending;
        //    _repo.Update(entity);
        //    await _repo.SaveChangesAsync();
        //}
        public async Task UpdateAsync(RecruitmentRequirementDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null)
            {
                throw new InvalidOperationException("Recruitment requirement not found.");
            }

            if (!currentRole.Contains("Manager") && !currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update recruitment requirements.");
            }

            if (entity.Status == RecruitmentStatus.Rejected ||
                entity.Status == RecruitmentStatus.Cancelled ||
                entity.Status == RecruitmentStatus.Completed)
            {
                throw new InvalidOperationException("Recruitment requirement cannot be updated in its current state.");
            }

            if ((RecruitmentStatus)dto.Status == RecruitmentStatus.Cancelled)
            {
                if (!currentRole.Contains("Manager") && !currentRole.Contains("HR"))
                {
                    throw new UnauthorizedAccessException("Only Manager or HR can cancel a recruitment requirement.");
                }

                entity.Status = RecruitmentStatus.Cancelled;
                _repo.Update(entity);
                await _repo.SaveChangesAsync();
                return;
            }

            if ((RecruitmentStatus)dto.Status == RecruitmentStatus.Completed)
            {
                if (!currentRole.Contains("HR"))
                {
                    throw new UnauthorizedAccessException("Only HR can mark a recruitment requirement as filled.");
                }

                if (entity.Status != RecruitmentStatus.Approved)
                {
                    throw new InvalidOperationException("Only approved recruitment requirements can be marked as filled.");
                }

                entity.Status = RecruitmentStatus.Completed;
                _repo.Update(entity);
                await _repo.SaveChangesAsync();
                await UpdateJobPostsStatusAsync(entity);
                return;
            }

            if (entity.Status == RecruitmentStatus.Pending)
            {
                if (!currentRole.Contains("Manager"))
                {
                    throw new UnauthorizedAccessException("Only Manager can edit pending recruitment requirements.");
                }

                _mapper.Map(dto, entity);

                var position = await _positionRepo.GetByIdAsync(dto.PositionId);
                if (position == null)
                {
                    throw new InvalidOperationException("Position not found.");
                }

                var employee = await _employeeRepo.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    throw new InvalidOperationException("Employee not found.");
                }

                entity.RecruitmentPositions = position;
                entity.EmployeeId = employeeId;
                entity.Employees = employee;
                entity.Status = RecruitmentStatus.Pending;

                _repo.Update(entity);
                await _repo.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Only pending or approved recruitment requirements can be updated.");
            }
        }
    }
}
