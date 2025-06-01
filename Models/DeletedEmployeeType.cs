using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class DeletedEmployeeType
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string TypeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
