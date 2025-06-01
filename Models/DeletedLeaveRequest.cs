using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedLeaveRequest
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Reason { get; set; }
        public int Status { get; set; }
        public int EmployeeId { get; set; }
        public int ApprovedById { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
