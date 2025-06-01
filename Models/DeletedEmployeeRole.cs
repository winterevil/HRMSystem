using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedEmployeeRole
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int EmployeeId { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
