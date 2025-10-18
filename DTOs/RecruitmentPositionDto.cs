using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class RecruitmentPositionDto
    {
        public int Id { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }
        public string? DepartmentName { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
