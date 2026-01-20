using HRMSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMSystem.Data
{
    public class DeletedDbContext : DbContext
    {
        public DeletedDbContext(DbContextOptions<DeletedDbContext> options) : base(options) { }
        public DbSet<DeletedAttendance> DeletedAttendances { get; set; }
        public DbSet<DeletedDepartment> DeletedDepartments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeletedEmployee>().ToTable("DeletedEmployees");
        }
        public DbSet<DeletedEmployeeRole> DeletedEmployeeRoles { get; set; }
        public DbSet<DeletedEmployeeType> DeletedEmployeeTypes { get; set; }
        public DbSet<DeletedJobPost> DeletedJobPosts { get; set; }
        public DbSet<DeletedLeaveRequest> DeletedLeaveRequests { get; set; }
        public DbSet<DeletedOvertimeRequest> DeletedOvertimeRequests { get; set; }
        public DbSet<DeletedRecruitmentPosition> DeletedRecruitmentPositions { get; set; }
        public DbSet<DeletedRecruitmentRequirement> DeletedRecruitmentRequirements { get; set; }
        public DbSet<DeletedRole> DeletedRoles { get; set; }
        public DbSet<DeletedNotification> DeletedNotifications { get; set; }
        public DbSet<DeletedNotificationRecipient> DeletedNotificationRecipients { get; set; }
    }
}
