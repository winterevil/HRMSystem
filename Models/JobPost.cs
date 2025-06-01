using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class JobPost
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("RecruimentRequirements")]
        public int RequirementId { get; set; }
        public RecruitmentRequirement RecruitmentRequirements { get; set; }
        [ForeignKey("Employees")]
        public int PostedById { get; set; }
        public Employee PostedBy { get; set; }
    }
}
