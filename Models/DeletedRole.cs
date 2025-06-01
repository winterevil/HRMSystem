using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class DeletedRole
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
