namespace LeaveManagement.Business.Models
{
    public class GoalCardTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TitleId { get; set; }
        public string TitleName { get; set; } = string.Empty;
        public int CreatedByEmployeeId { get; set; }
        public string CreatedByEmployeeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<GoalCardItemDto> Items { get; set; } = new List<GoalCardItemDto>();
    }

    public class GoalCardItemDto
    {
        public int Id { get; set; }
        public int GoalCardTemplateId { get; set; }
        public int GoalTypeId { get; set; }
        public string GoalTypeName { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Target80Percent { get; set; }
        public string? Target100Percent { get; set; }
        public string? Target120Percent { get; set; }
        public string? GoalDescription { get; set; }
        public int Order { get; set; }
    }

    public class CreateGoalCardTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public int TitleId { get; set; }
        public List<CreateGoalCardItemDto> Items { get; set; } = new List<CreateGoalCardItemDto>();
    }

    public class CreateGoalCardItemDto
    {
        public int GoalTypeId { get; set; }
        public string Goal { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Target80Percent { get; set; }
        public string? Target100Percent { get; set; }
        public string? Target120Percent { get; set; }
        public string? GoalDescription { get; set; }
        public int Order { get; set; }
    }

    public class UpdateGoalCardTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<UpdateGoalCardItemDto> Items { get; set; } = new List<UpdateGoalCardItemDto>();
    }

    public class UpdateGoalCardItemDto
    {
        public int? Id { get; set; } // null ise yeni item
        public int GoalTypeId { get; set; }
        public string Goal { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Target80Percent { get; set; }
        public string? Target100Percent { get; set; }
        public string? Target120Percent { get; set; }
        public string? GoalDescription { get; set; }
        public int Order { get; set; }
    }

    // Employee Goal Card DTOs
    public class EmployeeGoalCardDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int GoalCardTemplateId { get; set; }
        public string GoalCardTemplateName { get; set; } = string.Empty;
        public int CreatedByEmployeeId { get; set; }
        public string CreatedByEmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<EmployeeGoalCardItemDto> Items { get; set; } = new List<EmployeeGoalCardItemDto>();
    }

    public class EmployeeGoalCardItemDto
    {
        public int Id { get; set; }
        public int EmployeeGoalCardId { get; set; }
        public int GoalCardItemId { get; set; }
        public GoalCardItemDto GoalCardItem { get; set; } = null!;
        public DateTime? ActualCompletionDate { get; set; }
        public string? AchievementLevel { get; set; }
        public string? ManagerNotes { get; set; }
        public string? EmployeeNotes { get; set; }
    }

    public class CreateEmployeeGoalCardDto
    {
        public int EmployeeId { get; set; }
        public int GoalCardTemplateId { get; set; }
        public int Year { get; set; }
        public List<CreateEmployeeGoalCardItemDto> Items { get; set; } = new List<CreateEmployeeGoalCardItemDto>();
    }

    public class CreateEmployeeGoalCardItemDto
    {
        public int GoalCardItemId { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? AchievementLevel { get; set; }
        public string? ManagerNotes { get; set; }
        public string? EmployeeNotes { get; set; }
    }

    public class UpdateEmployeeGoalCardDto
    {
        public string Status { get; set; } = string.Empty;
        public List<UpdateEmployeeGoalCardItemDto> Items { get; set; } = new List<UpdateEmployeeGoalCardItemDto>();
    }

    public class UpdateEmployeeGoalCardItemDto
    {
        public int Id { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? AchievementLevel { get; set; }
        public string? ManagerNotes { get; set; }
        public string? EmployeeNotes { get; set; }
    }
}

