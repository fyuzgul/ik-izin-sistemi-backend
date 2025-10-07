namespace LeaveManagement.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Entity.Employee> Employees { get; }
        IRepository<Entity.Department> Departments { get; }
        IRepository<Entity.LeaveType> LeaveTypes { get; }
        IRepository<Entity.LeaveRequest> LeaveRequests { get; }
        IRepository<Entity.LeaveBalance> LeaveBalances { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}


