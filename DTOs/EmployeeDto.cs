using HRMSystem.Models;

namespace HRMSystem.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public string TypeName { get; set; }
        public string Status { get; set; }
        public int DepartmentId { get; set; } 
        public int EmployeeTypeId { get; set; }
    }
}
