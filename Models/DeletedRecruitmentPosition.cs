using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class DeletedRecruitmentPosition
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DepartmentId { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
