﻿using System.Text.Json.Serialization;

namespace HRMSystem.DTOs
{
    public class RecruitmentRequirementDto
    {
        public int Id { get; set; }
        public string Requirement { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PositionId { get; set; }
        public string PositionName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
    }
}
