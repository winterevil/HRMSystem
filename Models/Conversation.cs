using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = "Direct";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }

        public ICollection<ConversationMember> Members { get; set; }
    }
}
