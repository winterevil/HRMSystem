using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public enum RecruitmentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Completed = 3,
        Cancelled = 4
    }
    public class RecruitmentRequirement
    {
        [Key]
        public int Id { get; set; }
        public string Requirement { get; set; }
        public RecruitmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("RecruitmentPositions")]
        public int PositionId { get; set; }
        public RecruitmentPosition RecruitmentPositions { get; set; }
        [ForeignKey("Employees")]
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }

        public virtual ICollection<JobPost> JobPosts { get; set; }
    }
}
