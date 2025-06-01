using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public enum LeaveStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; }

        [ForeignKey("Employees")]
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }
        [ForeignKey("ApprovedBy")]
        public int? ApprovedById { get; set; }
        public Employee? ApprovedBy { get; set; }
    }
}
