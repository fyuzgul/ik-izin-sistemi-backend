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
                
                // Role relationship
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Unique constraints
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
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
            // Seed data removed - now handled by DbInitializer
            // DbInitializer will create initial data on first run
        }
    }
}


