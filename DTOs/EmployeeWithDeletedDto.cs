namespace HRMSystem.DTOs
{
    public class EmployeeWithDeletedDto : EmployeeDto
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedById { get; set; }
    }
}
