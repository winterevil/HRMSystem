using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class ConversationMember
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Conversation")]
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
