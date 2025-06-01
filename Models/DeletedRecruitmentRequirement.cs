using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedRecruitmentRequirement
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string Requirement { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PositionId { get; set; }
        public int EmployeeId { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
