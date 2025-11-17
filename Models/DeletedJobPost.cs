using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedJobPost
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RequirementId { get; set; }
        public int PostedById { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
        public int Status { get; set; }
    }
}
