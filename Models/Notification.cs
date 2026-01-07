using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public enum NotificationType
    {
        LeaveApproved = 0,
        LeaveRejected = 1,
        OvertimeApproved = 2,
        OvertimeRejected = 3,
        RecruitmentApproved = 4,
        RecruitmentRejected = 5,
        JobPostCreated = 6,
        DepartmentCreated = 7,
        EmployeeTypeCreated = 8,
        PositionCreated = 9,
        AutoCheckout = 10
    }
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
