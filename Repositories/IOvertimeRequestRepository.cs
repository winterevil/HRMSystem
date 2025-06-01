﻿using HRMSystem.Models;

namespace HRMSystem.Repositories
{
    public interface IOvertimeRequestRepository:IBaseRepository<OvertimeRequest, DeletedOvertimeRequest>
    {
        Task<IEnumerable<OvertimeRequest>> GetAllAsync();
        Task<OvertimeRequest?> GetByIdAsync(int id);
    }
}
