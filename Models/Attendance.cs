using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        public DateTime CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
        public DateTime CheckinDate { get; set; }

        [ForeignKey("Employees")]        
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }
    }
}
