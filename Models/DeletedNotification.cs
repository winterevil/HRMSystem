using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class DeletedNotification
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }

    }
}
