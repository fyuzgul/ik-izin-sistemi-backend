using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Business.Services
{
    public class GoalCardService : IGoalCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LeaveManagementDbContext _context;

        public GoalCardService(IUnitOfWork unitOfWork, LeaveManagementDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // Goal Types
        public async Task<IEnumerable<GoalTypeDto>> GetAllGoalTypesAsync()
        {
            var goalTypes = await _unitOfWork.GoalTypes.FindAsync(gt => gt.IsActive);
            return goalTypes.Select(MapToGoalTypeDto);
        }

        public async Task<GoalTypeDto?> GetGoalTypeByIdAsync(int id)
        {
            var goalType = await _unitOfWork.GoalTypes.GetByIdAsync(id);
            return goalType != null ? MapToGoalTypeDto(goalType) : null;
        }

        public async Task<GoalTypeDto> CreateGoalTypeAsync(CreateGoalTypeDto createDto)
        {
            var goalType = new GoalType
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.GoalTypes.AddAsync(goalType);
            await _unitOfWork.SaveChangesAsync();

            return MapToGoalTypeDto(goalType);
        }

        public async Task<bool> UpdateGoalTypeAsync(int id, UpdateGoalTypeDto updateDto)
        {
            var goalType = await _unitOfWork.GoalTypes.GetByIdAsync(id);
            if (goalType == null)
                return false;

            goalType.Name = updateDto.Name;
            goalType.Description = updateDto.Description;
            goalType.IsActive = updateDto.IsActive;
            goalType.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.GoalTypes.UpdateAsync(goalType);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteGoalTypeAsync(int id)
        {
            var goalType = await _unitOfWork.GoalTypes.GetByIdAsync(id);
            if (goalType == null)
                return false;

            // Soft delete
            goalType.IsActive = false;
            goalType.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.GoalTypes.UpdateAsync(goalType);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // Goal Card Templates
        public async Task<IEnumerable<GoalCardTemplateDto>> GetAllGoalCardTemplatesAsync()
        {
            var templates = await _context.GoalCardTemplates
                .Include(gct => gct.Department)
                .Include(gct => gct.Title)
                .Include(gct => gct.CreatedByEmployee)
                .Include(gct => gct.Items)
                    .ThenInclude(item => item.GoalType)
                .ToListAsync();

            return templates.Select(MapToGoalCardTemplateDto);
        }

        public async Task<GoalCardTemplateDto?> GetGoalCardTemplateByIdAsync(int id)
        {
            var template = await _context.GoalCardTemplates
                .Include(gct => gct.Department)
                .Include(gct => gct.Title)
                .Include(gct => gct.CreatedByEmployee)
                .Include(gct => gct.Items)
                    .ThenInclude(item => item.GoalType)
                .FirstOrDefaultAsync(gct => gct.Id == id);

            return template != null ? MapToGoalCardTemplateDto(template) : null;
        }

        public async Task<IEnumerable<GoalCardTemplateDto>> GetGoalCardTemplatesByDepartmentAndTitleAsync(int departmentId, int titleId)
        {
            var templates = await _context.GoalCardTemplates
                .Include(gct => gct.Department)
                .Include(gct => gct.Title)
                .Include(gct => gct.CreatedByEmployee)
                .Include(gct => gct.Items)
                    .ThenInclude(item => item.GoalType)
                .Where(gct => gct.DepartmentId == departmentId && gct.TitleId == titleId && gct.IsActive)
                .ToListAsync();

            return templates.Select(MapToGoalCardTemplateDto);
        }

        public async Task<GoalCardTemplateDto> CreateGoalCardTemplateAsync(CreateGoalCardTemplateDto createDto, int createdByEmployeeId)
        {
            // Validate department and title exist
            var department = await _unitOfWork.Departments.GetByIdAsync(createDto.DepartmentId);
            if (department == null)
                throw new ArgumentException("Department not found");

            var title = await _unitOfWork.Titles.GetByIdAsync(createDto.TitleId);
            if (title == null)
                throw new ArgumentException("Title not found");

            // Check if active template already exists for this department and title
            var existingTemplate = await _context.GoalCardTemplates
                .FirstOrDefaultAsync(gct => 
                    gct.DepartmentId == createDto.DepartmentId && 
                    gct.TitleId == createDto.TitleId && 
                    gct.IsActive);

            if (existingTemplate != null)
            {
                // Deactivate existing template
                existingTemplate.IsActive = false;
                existingTemplate.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.GoalCardTemplates.UpdateAsync(existingTemplate);
            }

            // Create new template
            var template = new GoalCardTemplate
            {
                Name = createDto.Name,
                Description = createDto.Description,
                DepartmentId = createDto.DepartmentId,
                TitleId = createDto.TitleId,
                CreatedByEmployeeId = createdByEmployeeId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.GoalCardTemplates.AddAsync(template);
            await _unitOfWork.SaveChangesAsync();

            // Add items
            foreach (var itemDto in createDto.Items.OrderBy(i => i.Order))
            {
                var goalType = await _unitOfWork.GoalTypes.GetByIdAsync(itemDto.GoalTypeId);
                if (goalType == null)
                    throw new ArgumentException($"Goal type with id {itemDto.GoalTypeId} not found");

                var item = new GoalCardItem
                {
                    GoalCardTemplateId = template.Id,
                    GoalTypeId = itemDto.GoalTypeId,
                    Goal = itemDto.Goal,
                    TargetDate = itemDto.TargetDate,
                    Weight = itemDto.Weight,
                    Target80Percent = itemDto.Target80Percent,
                    Target100Percent = itemDto.Target100Percent,
                    Target120Percent = itemDto.Target120Percent,
                    GoalDescription = itemDto.GoalDescription,
                    Order = itemDto.Order,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.GoalCardItems.AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();

            // Ensure template is active after saving
            template.IsActive = true;
            await _unitOfWork.GoalCardTemplates.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();

            return await GetGoalCardTemplateByIdAsync(template.Id) ?? throw new Exception("Failed to create template");
        }

        public async Task<bool> UpdateGoalCardTemplateAsync(int id, UpdateGoalCardTemplateDto updateDto)
        {
            var template = await _context.GoalCardTemplates
                .Include(gct => gct.Items)
                .FirstOrDefaultAsync(gct => gct.Id == id);

            if (template == null)
                return false;

            template.Name = updateDto.Name;
            template.Description = updateDto.Description;
            template.IsActive = updateDto.IsActive;
            template.UpdatedDate = DateTime.UtcNow;

            // Update or create items
            var existingItemIds = updateDto.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();
            var itemsToDelete = template.Items.Where(item => !existingItemIds.Contains(item.Id)).ToList();

            foreach (var itemToDelete in itemsToDelete)
            {
                await _unitOfWork.GoalCardItems.DeleteAsync(itemToDelete);
            }

            foreach (var itemDto in updateDto.Items.OrderBy(i => i.Order))
            {
                if (itemDto.Id.HasValue)
                {
                    // Update existing item
                    var existingItem = template.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value);
                    if (existingItem != null)
                    {
                        existingItem.GoalTypeId = itemDto.GoalTypeId;
                        existingItem.Goal = itemDto.Goal;
                        existingItem.TargetDate = itemDto.TargetDate;
                        existingItem.Weight = itemDto.Weight;
                        existingItem.Target80Percent = itemDto.Target80Percent;
                        existingItem.Target100Percent = itemDto.Target100Percent;
                        existingItem.Target120Percent = itemDto.Target120Percent;
                        existingItem.GoalDescription = itemDto.GoalDescription;
                        existingItem.Order = itemDto.Order;
                        existingItem.UpdatedDate = DateTime.UtcNow;

                        await _unitOfWork.GoalCardItems.UpdateAsync(existingItem);
                    }
                }
                else
                {
                    // Create new item
                    var goalType = await _unitOfWork.GoalTypes.GetByIdAsync(itemDto.GoalTypeId);
                    if (goalType == null)
                        throw new ArgumentException($"Goal type with id {itemDto.GoalTypeId} not found");

                    var newItem = new GoalCardItem
                    {
                        GoalCardTemplateId = template.Id,
                        GoalTypeId = itemDto.GoalTypeId,
                        Goal = itemDto.Goal,
                        TargetDate = itemDto.TargetDate,
                        Weight = itemDto.Weight,
                        Target80Percent = itemDto.Target80Percent,
                        Target100Percent = itemDto.Target100Percent,
                        Target120Percent = itemDto.Target120Percent,
                        GoalDescription = itemDto.GoalDescription,
                        Order = itemDto.Order,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.GoalCardItems.AddAsync(newItem);
                }
            }

            await _unitOfWork.GoalCardTemplates.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteGoalCardTemplateAsync(int id)
        {
            var template = await _unitOfWork.GoalCardTemplates.GetByIdAsync(id);
            if (template == null)
                return false;

            // Soft delete
            template.IsActive = false;
            template.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.GoalCardTemplates.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // Employee Goal Cards
        public async Task<IEnumerable<EmployeeGoalCardDto>> GetEmployeeGoalCardsByEmployeeIdAsync(int employeeId)
        {
            var goalCards = await _context.EmployeeGoalCards
                .Include(egc => egc.Employee)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Department)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Title)
                .Include(egc => egc.CreatedByEmployee)
                .Include(egc => egc.Items)
                    .ThenInclude(item => item.GoalCardItem)
                        .ThenInclude(gci => gci.GoalType)
                .Where(egc => egc.EmployeeId == employeeId)
                .OrderByDescending(egc => egc.Year)
                .ThenByDescending(egc => egc.CreatedDate)
                .ToListAsync();

            return goalCards.Select(MapToEmployeeGoalCardDto);
        }

        public async Task<IEnumerable<EmployeeGoalCardDto>> GetEmployeeGoalCardsByManagerIdAsync(int managerId)
        {
            // Get all subordinates of the manager
            var subordinates = await _context.Employees
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            var goalCards = await _context.EmployeeGoalCards
                .Include(egc => egc.Employee)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Department)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Title)
                .Include(egc => egc.CreatedByEmployee)
                .Include(egc => egc.Items)
                    .ThenInclude(item => item.GoalCardItem)
                        .ThenInclude(gci => gci.GoalType)
                .Where(egc => subordinates.Contains(egc.EmployeeId))
                .OrderByDescending(egc => egc.Year)
                .ThenByDescending(egc => egc.CreatedDate)
                .ToListAsync();

            return goalCards.Select(MapToEmployeeGoalCardDto);
        }

        public async Task<EmployeeGoalCardDto?> GetEmployeeGoalCardByIdAsync(int id)
        {
            var goalCard = await _context.EmployeeGoalCards
                .Include(egc => egc.Employee)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Department)
                .Include(egc => egc.GoalCardTemplate)
                    .ThenInclude(gct => gct.Title)
                .Include(egc => egc.CreatedByEmployee)
                .Include(egc => egc.Items)
                    .ThenInclude(item => item.GoalCardItem)
                        .ThenInclude(gci => gci.GoalType)
                .FirstOrDefaultAsync(egc => egc.Id == id);

            return goalCard != null ? MapToEmployeeGoalCardDto(goalCard) : null;
        }

        public async Task<EmployeeGoalCardDto> CreateEmployeeGoalCardAsync(CreateEmployeeGoalCardDto createDto, int createdByEmployeeId)
        {
            // Validate employee exists
            var employee = await _unitOfWork.Employees.GetByIdAsync(createDto.EmployeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            // Validate template exists
            var template = await _context.GoalCardTemplates
                .Include(gct => gct.Items)
                .FirstOrDefaultAsync(gct => gct.Id == createDto.GoalCardTemplateId && gct.IsActive);

            if (template == null)
                throw new ArgumentException("Goal card template not found or inactive");

            // Check if goal card already exists for this employee, template, and year
            var existingGoalCard = await _context.EmployeeGoalCards
                .FirstOrDefaultAsync(egc => 
                    egc.EmployeeId == createDto.EmployeeId && 
                    egc.GoalCardTemplateId == createDto.GoalCardTemplateId && 
                    egc.Year == createDto.Year);

            if (existingGoalCard != null)
                throw new InvalidOperationException("A goal card already exists for this employee, template, and year");

            // Create employee goal card
            var goalCard = new EmployeeGoalCard
            {
                EmployeeId = createDto.EmployeeId,
                GoalCardTemplateId = createDto.GoalCardTemplateId,
                CreatedByEmployeeId = createdByEmployeeId,
                Year = createDto.Year,
                Status = "Draft",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.EmployeeGoalCards.AddAsync(goalCard);
            await _unitOfWork.SaveChangesAsync();

            // Create items from template
            foreach (var templateItem in template.Items.OrderBy(i => i.Order))
            {
                var itemDto = createDto.Items.FirstOrDefault(i => i.GoalCardItemId == templateItem.Id);
                
                var goalCardItem = new EmployeeGoalCardItem
                {
                    EmployeeGoalCardId = goalCard.Id,
                    GoalCardItemId = templateItem.Id,
                    ActualCompletionDate = itemDto?.ActualCompletionDate,
                    AchievementLevel = itemDto?.AchievementLevel,
                    ManagerNotes = itemDto?.ManagerNotes,
                    EmployeeNotes = itemDto?.EmployeeNotes,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.EmployeeGoalCardItems.AddAsync(goalCardItem);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetEmployeeGoalCardByIdAsync(goalCard.Id) ?? throw new Exception("Failed to create employee goal card");
        }

        public async Task<bool> UpdateEmployeeGoalCardAsync(int id, UpdateEmployeeGoalCardDto updateDto)
        {
            var goalCard = await _context.EmployeeGoalCards
                .Include(egc => egc.Items)
                .FirstOrDefaultAsync(egc => egc.Id == id);

            if (goalCard == null)
                return false;

            goalCard.Status = updateDto.Status;
            goalCard.UpdatedDate = DateTime.UtcNow;

            if (updateDto.Status == "Completed")
            {
                goalCard.CompletedDate = DateTime.UtcNow;
            }

            // Update items
            foreach (var itemDto in updateDto.Items)
            {
                var item = goalCard.Items.FirstOrDefault(i => i.Id == itemDto.Id);
                if (item != null)
                {
                    item.ActualCompletionDate = itemDto.ActualCompletionDate;
                    item.AchievementLevel = itemDto.AchievementLevel;
                    item.ManagerNotes = itemDto.ManagerNotes;
                    item.EmployeeNotes = itemDto.EmployeeNotes;
                    item.UpdatedDate = DateTime.UtcNow;

                    await _unitOfWork.EmployeeGoalCardItems.UpdateAsync(item);
                }
            }

            await _unitOfWork.EmployeeGoalCards.UpdateAsync(goalCard);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEmployeeGoalCardAsync(int id)
        {
            var goalCard = await _unitOfWork.EmployeeGoalCards.GetByIdAsync(id);
            if (goalCard == null)
                return false;

            // Delete items first (cascade delete should handle this, but being explicit)
            var items = await _context.EmployeeGoalCardItems
                .Where(egci => egci.EmployeeGoalCardId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                await _unitOfWork.EmployeeGoalCardItems.DeleteAsync(item);
            }

            await _unitOfWork.EmployeeGoalCards.DeleteAsync(goalCard);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // Mapping methods
        private GoalTypeDto MapToGoalTypeDto(GoalType goalType)
        {
            return new GoalTypeDto
            {
                Id = goalType.Id,
                Name = goalType.Name,
                Description = goalType.Description,
                IsActive = goalType.IsActive,
                CreatedDate = goalType.CreatedDate
            };
        }

        private GoalCardTemplateDto MapToGoalCardTemplateDto(GoalCardTemplate template)
        {
            return new GoalCardTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                DepartmentId = template.DepartmentId,
                DepartmentName = template.Department.Name,
                TitleId = template.TitleId,
                TitleName = template.Title.Name,
                CreatedByEmployeeId = template.CreatedByEmployeeId,
                CreatedByEmployeeName = $"{template.CreatedByEmployee.FirstName} {template.CreatedByEmployee.LastName}",
                IsActive = template.IsActive,
                CreatedDate = template.CreatedDate,
                Items = template.Items.OrderBy(i => i.Order).Select(MapToGoalCardItemDto).ToList()
            };
        }

        private GoalCardItemDto MapToGoalCardItemDto(GoalCardItem item)
        {
            return new GoalCardItemDto
            {
                Id = item.Id,
                GoalCardTemplateId = item.GoalCardTemplateId,
                GoalTypeId = item.GoalTypeId,
                GoalTypeName = item.GoalType.Name,
                Goal = item.Goal,
                TargetDate = item.TargetDate,
                Weight = item.Weight,
                Target80Percent = item.Target80Percent,
                Target100Percent = item.Target100Percent,
                Target120Percent = item.Target120Percent,
                GoalDescription = item.GoalDescription,
                Order = item.Order
            };
        }

        private EmployeeGoalCardDto MapToEmployeeGoalCardDto(EmployeeGoalCard goalCard)
        {
            return new EmployeeGoalCardDto
            {
                Id = goalCard.Id,
                EmployeeId = goalCard.EmployeeId,
                EmployeeName = $"{goalCard.Employee.FirstName} {goalCard.Employee.LastName}",
                GoalCardTemplateId = goalCard.GoalCardTemplateId,
                GoalCardTemplateName = goalCard.GoalCardTemplate.Name,
                CreatedByEmployeeId = goalCard.CreatedByEmployeeId,
                CreatedByEmployeeName = $"{goalCard.CreatedByEmployee.FirstName} {goalCard.CreatedByEmployee.LastName}",
                Year = goalCard.Year,
                Status = goalCard.Status,
                CreatedDate = goalCard.CreatedDate,
                CompletedDate = goalCard.CompletedDate,
                Items = goalCard.Items.Select(MapToEmployeeGoalCardItemDto).ToList()
            };
        }

        private EmployeeGoalCardItemDto MapToEmployeeGoalCardItemDto(EmployeeGoalCardItem item)
        {
            return new EmployeeGoalCardItemDto
            {
                Id = item.Id,
                EmployeeGoalCardId = item.EmployeeGoalCardId,
                GoalCardItemId = item.GoalCardItemId,
                GoalCardItem = MapToGoalCardItemDto(item.GoalCardItem),
                ActualCompletionDate = item.ActualCompletionDate,
                AchievementLevel = item.AchievementLevel,
                ManagerNotes = item.ManagerNotes,
                EmployeeNotes = item.EmployeeNotes
            };
        }
    }
}

