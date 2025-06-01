using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class OvertimeRequestDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Reason { get; set; }
        public int Status { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
    }
}
