using System.ComponentModel.DataAnnotations;

namespace HRMSystem.Models
{
    public class EmployeeType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TypeName { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public bool IsSystemOnly { get; set; } = false;

    }
}
