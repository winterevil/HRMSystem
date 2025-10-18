using System.Text.Json.Serialization;
using HRMSystem.Models;

namespace HRMSystem.DTOs
{
    public class EmployeeUpdateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Password { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public int? EmployeeTypeId { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
