using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public enum EmployeeStatus
    {
        Active = 0,
        OnLeave = 1,
        Resigned = 2,
        Retired = 3,
        Probation = 4
    }
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string HashPassword { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get;set; }
        public EmployeeStatus Status { get; set; }

        [ForeignKey("Departments")]
        public int? DepartmentId { get; set; }
        public Department Departments { get; set; }
        [ForeignKey("EmployeeTypes")]
        public int? EmployeeTypeId { get; set; }
        public EmployeeType EmployeeTypes { get; set; }

        public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; }
        public virtual ICollection<OvertimeRequest> OvertimeRequests { get; set; }
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; }
        public virtual ICollection<RecruitmentRequirement> RecruitmentRequirements { get; set; }
        public virtual ICollection<JobPost> JobPosts { get; set; }
        public virtual ICollection<Attendance> Attendance { get; set; }
    }
}
