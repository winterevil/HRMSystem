using System.Security.Claims;
using AutoMapper;
using HRMSystem.DTOs;
using HRMSystem.Models;
using HRMSystem.Repositories;

namespace HRMSystem.Services
{
    public class JobPostService : IJobPostService
    {
        private readonly IJobPostRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IMapper _mapper;
        private readonly IRecruitmentRequirementRepository _recruitmentRepo;
        public JobPostService(IJobPostRepository repo, IMapper mapper, IEmployeeRepository employeeRepo, IRecruitmentRequirementRepository recruitmentRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _employeeRepo = employeeRepo;
            _recruitmentRepo = recruitmentRepo;
        }
        public async Task CreateAsync(JobPostDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to create a job post.");
            }
            var requirement = await _recruitmentRepo.GetByIdAsync(dto.RequirementId);
            if (requirement == null || requirement.Status != RecruitmentStatus.Approved)
            {
                throw new InvalidOperationException("The associated requirement must be approved before creating a job post.");
            }
            var posterId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var poster = await _employeeRepo.GetByIdAsync(posterId);
            if (poster == null)
            {
                throw new InvalidOperationException("Poster not found.");
            }
            var jobPost = _mapper.Map<JobPost>(dto);
            jobPost.PostedBy = poster;
            jobPost.CreatedAt = DateTime.UtcNow.AddHours(7);
            jobPost.RecruitmentRequirements = requirement;
            jobPost.RequirementId = requirement.Id;
            jobPost.PostedById = poster.Id;

            await _repo.AddAsync(jobPost);
            await _repo.SaveChangesAsync();

        }

        public async Task DeleteAsync(int id, ClaimsPrincipal user)
        {
            var currentRole = user.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to delete a job post.");
            }
            var existingPost = await _repo.GetByIdAsync(id);
            if (existingPost == null)
            {
                throw new InvalidOperationException($"Job post with ID {id} does not exist.");
            }
            var deleted = new DeletedJobPost
            {
                Id = existingPost.Id,
                Title = existingPost.Title,
                Content = existingPost.Content,
                PostedById = existingPost.PostedById,
                CreatedAt = existingPost.CreatedAt,
                RequirementId = existingPost.RequirementId,
                DeletedById = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                DeletedAt = DateTime.UtcNow.AddHours(7)
            };

            await _repo.DeleteWithArchiveAsync(existingPost, deleted);
            await _repo.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPostDto>> GetAllAsync()
        {
            var jobPosts = await _repo.GetAllAsync();
            var job = jobPosts.OrderByDescending(j => j.CreatedAt)
                           .Select(j => _mapper.Map<JobPostDto>(j));
            if (job == null || !job.Any())
            {
                throw new InvalidOperationException("No job posts found.");
            }
            return job;
        }

        public async Task<JobPostDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Job post is not found.");
            }
            return _mapper.Map<JobPostDto>(entity);
        }

        public async Task UpdateAsync(JobPostDto dto, ClaimsPrincipal user)
        {
            var currentRole = user.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;
            if (!currentRole.Contains("HR"))
            {
                throw new UnauthorizedAccessException("You do not have permission to update a job post.");
            }
            var existingPost = await _repo.GetByIdAsync(dto.Id);
            if (existingPost == null)
            {
                throw new InvalidOperationException($"Job post with ID {dto.Id} does not exist.");
            }
            var requirement = await _recruitmentRepo.GetByIdAsync(dto.RequirementId);
            if (requirement == null || requirement.Status != RecruitmentStatus.Approved)
            {
                throw new InvalidOperationException("The associated requirement must be approved before updating a job post.");
            }
            var posterId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var poster = await _employeeRepo.GetByIdAsync(posterId);
            if (poster == null)
            {
                throw new InvalidOperationException("Poster not found.");
            }
            _mapper.Map(dto, existingPost);
            existingPost.RecruitmentRequirements = requirement;
            existingPost.RequirementId = requirement.Id;
            existingPost.PostedBy = poster;
            existingPost.PostedById = poster.Id;
            _repo.Update(existingPost);
            await _repo.SaveChangesAsync();
        }
    }
}
