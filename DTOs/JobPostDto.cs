using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class JobPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RequirementId { get; set; }
        public string Requirement { get; set; }
        public int PostedById { get; set; }
        public string PostedBy { get; set; }
    }
}
