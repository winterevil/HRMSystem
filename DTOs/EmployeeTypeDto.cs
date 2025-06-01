using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class EmployeeTypeDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
