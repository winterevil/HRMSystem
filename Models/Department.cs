using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<RecruitmentPosition> RecruitmentPositions { get; set; }
    }
}
