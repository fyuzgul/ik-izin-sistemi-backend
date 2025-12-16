using Microsoft.EntityFrameworkCore.Storage;

namespace LeaveManagement.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Entity.LeaveManagementDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(Entity.LeaveManagementDbContext context)
        {
            _context = context;
            Employees = new Repository<Entity.Employee>(_context);
            Departments = new Repository<Entity.Department>(_context);
            LeaveTypes = new Repository<Entity.LeaveType>(_context);
            LeaveRequests = new Repository<Entity.LeaveRequest>(_context);
            LeaveBalances = new Repository<Entity.LeaveBalance>(_context);
            Titles = new Repository<Entity.Title>(_context);
            GoalTypes = new Repository<Entity.GoalType>(_context);
            GoalCardTemplates = new Repository<Entity.GoalCardTemplate>(_context);
            GoalCardItems = new Repository<Entity.GoalCardItem>(_context);
            EmployeeGoalCards = new Repository<Entity.EmployeeGoalCard>(_context);
            EmployeeGoalCardItems = new Repository<Entity.EmployeeGoalCardItem>(_context);
            Holidays = new Repository<Entity.Holiday>(_context);
        }

        public IRepository<Entity.Employee> Employees { get; }
        public IRepository<Entity.Department> Departments { get; }
        public IRepository<Entity.LeaveType> LeaveTypes { get; }
        public IRepository<Entity.LeaveRequest> LeaveRequests { get; }
        public IRepository<Entity.LeaveBalance> LeaveBalances { get; }
        public IRepository<Entity.Title> Titles { get; }
        public IRepository<Entity.GoalType> GoalTypes { get; }
        public IRepository<Entity.GoalCardTemplate> GoalCardTemplates { get; }
        public IRepository<Entity.GoalCardItem> GoalCardItems { get; }
        public IRepository<Entity.EmployeeGoalCard> EmployeeGoalCards { get; }
        public IRepository<Entity.EmployeeGoalCardItem> EmployeeGoalCardItems { get; }
        public IRepository<Entity.Holiday> Holidays { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}


