using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<OvertimeRequest> OvertimeRequests { get; set; }
        public DbSet<RecruitmentPosition> RecruitmentPositions { get; set; }
        public DbSet<RecruitmentRequirement> RecruitmentRequirements { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // LeaveRequest
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employees)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.ApprovedBy)
                .WithMany()
                .HasForeignKey(l => l.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // OvertimeRequest
            modelBuilder.Entity<OvertimeRequest>()
                .HasOne(o => o.Employees)
                .WithMany(e => e.OvertimeRequests)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OvertimeRequest>()
                .HasOne(o => o.ApprovedBy)
                .WithMany()
                .HasForeignKey(o => o.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // JobPost
            modelBuilder.Entity<JobPost>()
                .HasOne(j => j.RecruitmentRequirements)
                .WithMany(r => r.JobPosts)
                .HasForeignKey(j => j.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);  

            modelBuilder.Entity<JobPost>()
                .HasOne(j => j.PostedBy)
                .WithMany(e => e.JobPosts)
                .HasForeignKey(j => j.PostedById)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
