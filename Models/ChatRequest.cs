using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class ChatRequest
    {
        [Key]
        public int Id { get; set; }
        public int FromEmployeeId { get; set; }
        public int ToEmployeeId { get; set; }
        [ForeignKey(nameof(FromEmployeeId))]
        public Employee FromEmployee { get; set; }   

        [ForeignKey(nameof(ToEmployeeId))]
        public Employee ToEmployee { get; set; }     
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
    }
}
