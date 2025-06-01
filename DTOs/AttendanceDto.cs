using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public DateTime CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public DateTime CheckinDate { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        }
}
