using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedNotificationRecipient
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public int EmployeeId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
