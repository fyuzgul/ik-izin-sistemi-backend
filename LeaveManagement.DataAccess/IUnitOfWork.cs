namespace LeaveManagement.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Entity.Employee> Employees { get; }
        IRepository<Entity.Department> Departments { get; }
        IRepository<Entity.LeaveType> LeaveTypes { get; }
        IRepository<Entity.LeaveRequest> LeaveRequests { get; }
        IRepository<Entity.LeaveBalance> LeaveBalances { get; }
        IRepository<Entity.Title> Titles { get; }
        IRepository<Entity.GoalType> GoalTypes { get; }
        IRepository<Entity.GoalCardTemplate> GoalCardTemplates { get; }
        IRepository<Entity.GoalCardItem> GoalCardItems { get; }
        IRepository<Entity.EmployeeGoalCard> EmployeeGoalCards { get; }
        IRepository<Entity.EmployeeGoalCardItem> EmployeeGoalCardItems { get; }
        IRepository<Entity.Holiday> Holidays { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}


