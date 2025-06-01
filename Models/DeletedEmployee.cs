using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMSystem.Models
{
    public class DeletedEmployee
    {
        [Key]
        public int DeletedId { get; set; }
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HashPassword { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get;set; }
        public int Status { get; set; }
        public int DepartmentId { get; set; }
        public int EmployeeTypeId { get; set; }
        public int DeletedById { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
