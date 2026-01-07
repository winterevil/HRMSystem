using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class NotificationRecipient
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Notifications")]
        public int NotificationId { get; set; }
        public Notification Notifications { get; set; }

        [ForeignKey("Employees")]
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
