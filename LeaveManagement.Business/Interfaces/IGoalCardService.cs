using LeaveManagement.Business.Models;

namespace LeaveManagement.Business.Interfaces
{
    public interface IGoalCardService
    {
        // Goal Types
        Task<IEnumerable<GoalTypeDto>> GetAllGoalTypesAsync();
        Task<GoalTypeDto?> GetGoalTypeByIdAsync(int id);
        Task<GoalTypeDto> CreateGoalTypeAsync(CreateGoalTypeDto createDto);
        Task<bool> UpdateGoalTypeAsync(int id, UpdateGoalTypeDto updateDto);
        Task<bool> DeleteGoalTypeAsync(int id);

        // Goal Card Templates
        Task<IEnumerable<GoalCardTemplateDto>> GetAllGoalCardTemplatesAsync();
        Task<GoalCardTemplateDto?> GetGoalCardTemplateByIdAsync(int id);
        Task<IEnumerable<GoalCardTemplateDto>> GetGoalCardTemplatesByDepartmentAndTitleAsync(int departmentId, int titleId);
        Task<GoalCardTemplateDto> CreateGoalCardTemplateAsync(CreateGoalCardTemplateDto createDto, int createdByEmployeeId);
        Task<bool> UpdateGoalCardTemplateAsync(int id, UpdateGoalCardTemplateDto updateDto);
        Task<bool> DeleteGoalCardTemplateAsync(int id);

        // Employee Goal Cards
        Task<IEnumerable<EmployeeGoalCardDto>> GetEmployeeGoalCardsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<EmployeeGoalCardDto>> GetEmployeeGoalCardsByManagerIdAsync(int managerId);
        Task<EmployeeGoalCardDto?> GetEmployeeGoalCardByIdAsync(int id);
        Task<EmployeeGoalCardDto> CreateEmployeeGoalCardAsync(CreateEmployeeGoalCardDto createDto, int createdByEmployeeId);
        Task<bool> UpdateEmployeeGoalCardAsync(int id, UpdateEmployeeGoalCardDto updateDto);
        Task<bool> DeleteEmployeeGoalCardAsync(int id);
    }
}

