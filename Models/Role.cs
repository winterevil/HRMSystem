using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; }
    }
}
