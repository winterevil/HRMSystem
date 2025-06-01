using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class RecruitmentPosition
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PositionName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [ForeignKey("Departments")]
        public int DepartmentId { get; set; }
        public Department Departments { get; set; }

    }
}
