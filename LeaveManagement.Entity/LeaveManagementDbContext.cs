using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Entity
{
    public class LeaveManagementDbContext : DbContext
    {
        public LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee configurations
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(50);
                
                // Self-referencing relationship for Manager
                entity.HasOne(e => e.Manager)
                      .WithMany(e => e.Subordinates)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Department relationship
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Department configurations
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Description).HasMaxLength(500);
                
                entity.HasOne(d => d.Manager)
                      .WithMany()
                      .HasForeignKey(d => d.ManagerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // LeaveType configurations
            modelBuilder.Entity<LeaveType>(entity =>
            {
                entity.HasKey(lt => lt.Id);
                entity.Property(lt => lt.Name).IsRequired().HasMaxLength(100);
                entity.Property(lt => lt.Description).HasMaxLength(500);
            });

            // LeaveRequest configurations
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(lr => lr.Id);
                entity.Property(lr => lr.Reason).HasMaxLength(1000);
                entity.Property(lr => lr.DepartmentManagerComments).HasMaxLength(500);
                entity.Property(lr => lr.HrManagerComments).HasMaxLength(500);
                
                entity.HasOne(lr => lr.Employee)
                      .WithMany(e => e.LeaveRequests)
                      .HasForeignKey(lr => lr.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(lr => lr.LeaveType)
                      .WithMany(lt => lt.LeaveRequests)
                      .HasForeignKey(lr => lr.LeaveTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(lr => lr.DepartmentManager)
                      .WithMany()
                      .HasForeignKey(lr => lr.DepartmentManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(lr => lr.HrManager)
                      .WithMany()
                      .HasForeignKey(lr => lr.HrManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveBalance configurations
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(lb => lb.Id);
                
                entity.HasOne(lb => lb.Employee)
                      .WithMany(e => e.LeaveBalances)
                      .HasForeignKey(lb => lb.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(lb => lb.LeaveType)
                      .WithMany(lt => lt.LeaveBalances)
                      .HasForeignKey(lb => lb.LeaveTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Unique constraint for Employee, LeaveType, and Year
                entity.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year })
                      .IsUnique();
            });

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Employee)
                      .WithOne()
                      .HasForeignKey<User>(u => u.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Role configurations
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "SystemAdmin", Description = "Sistem Yöneticisi", CanManageEmployees = true, CanManageDepartments = true, CanManageLeaveTypes = true, CanApproveLeaveRequests = true, CanViewAllLeaveRequests = true, CanManageSystemSettings = true },
                new Role { Id = 2, Name = "HrManager", Description = "İK Müdürü", CanManageEmployees = true, CanManageDepartments = false, CanManageLeaveTypes = true, CanApproveLeaveRequests = true, CanViewAllLeaveRequests = true, CanManageSystemSettings = false },
                new Role { Id = 3, Name = "DepartmentManager", Description = "Departman Yöneticisi", CanManageEmployees = false, CanManageDepartments = false, CanManageLeaveTypes = false, CanApproveLeaveRequests = true, CanViewAllLeaveRequests = false, CanManageSystemSettings = false },
                new Role { Id = 4, Name = "Employee", Description = "Çalışan", CanManageEmployees = false, CanManageDepartments = false, CanManageLeaveTypes = false, CanApproveLeaveRequests = false, CanViewAllLeaveRequests = false, CanManageSystemSettings = false }
            );

            // Seed LeaveTypes
            modelBuilder.Entity<LeaveType>().HasData(
                new LeaveType { Id = 1, Name = "Annual Leave", Description = "Yearly vacation leave", MaxDaysPerYear = 30, RequiresApproval = true, IsPaid = true },
                new LeaveType { Id = 2, Name = "Sick Leave", Description = "Medical leave", MaxDaysPerYear = 10, RequiresApproval = true, IsPaid = true },
                new LeaveType { Id = 3, Name = "Maternity Leave", Description = "Maternity leave", MaxDaysPerYear = 90, RequiresApproval = true, IsPaid = true },
                new LeaveType { Id = 4, Name = "Paternity Leave", Description = "Paternity leave", MaxDaysPerYear = 15, RequiresApproval = true, IsPaid = true },
                new LeaveType { Id = 5, Name = "Emergency Leave", Description = "Emergency situations", MaxDaysPerYear = 5, RequiresApproval = true, IsPaid = true }
            );

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Human Resources", Description = "HR Department" },
                new Department { Id = 2, Name = "Information Technology", Description = "IT Department" },
                new Department { Id = 3, Name = "Finance", Description = "Finance Department" },
                new Department { Id = 4, Name = "Marketing", Description = "Marketing Department" }
            );

            // Seed System Admin User
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@company.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), EmployeeId = 1, RoleId = 1, IsActive = true }
            );

            // Seed System Admin Employee
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, FirstName = "System", LastName = "Administrator", Email = "admin@company.com", EmployeeNumber = "EMP001", PhoneNumber = "555-0001", HireDate = DateTime.UtcNow, DepartmentId = 1, IsActive = true }
            );
        }
    }
}


