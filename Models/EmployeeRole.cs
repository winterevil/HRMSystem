using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class EmployeeRole
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Roles")]
        public int RoleId { get; set; }
        public Role Roles { get; set; }
        [ForeignKey("Employees")]
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }
    }
}
