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
        public DbSet<Title> Titles { get; set; }
        public DbSet<GoalType> GoalTypes { get; set; }
        public DbSet<GoalCardTemplate> GoalCardTemplates { get; set; }
        public DbSet<GoalCardItem> GoalCardItems { get; set; }
        public DbSet<EmployeeGoalCard> EmployeeGoalCards { get; set; }
        public DbSet<EmployeeGoalCardItem> EmployeeGoalCardItems { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

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
                
                // Title relationship
                entity.HasOne(e => e.Title)
                      .WithMany()
                      .HasForeignKey(e => e.TitleId)
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


            // Title configurations
            modelBuilder.Entity<Title>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Name).IsUnique();
            });

            // GoalType configurations
            modelBuilder.Entity<GoalType>(entity =>
            {
                entity.HasKey(gt => gt.Id);
                entity.Property(gt => gt.Name).IsRequired().HasMaxLength(100);
                entity.Property(gt => gt.Description).HasMaxLength(500);
            });

            // GoalCardTemplate configurations
            modelBuilder.Entity<GoalCardTemplate>(entity =>
            {
                entity.HasKey(gct => gct.Id);
                entity.Property(gct => gct.Name).IsRequired().HasMaxLength(200);
                entity.Property(gct => gct.Description).HasMaxLength(500);
                
                entity.HasOne(gct => gct.Department)
                      .WithMany()
                      .HasForeignKey(gct => gct.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(gct => gct.Title)
                      .WithMany()
                      .HasForeignKey(gct => gct.TitleId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(gct => gct.CreatedByEmployee)
                      .WithMany()
                      .HasForeignKey(gct => gct.CreatedByEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Unique constraint: Aynı departman ve pozisyon için tek bir aktif şablon olabilir
                entity.HasIndex(gct => new { gct.DepartmentId, gct.TitleId, gct.IsActive })
                      .IsUnique()
                      .HasFilter("\"IsActive\" = true");
            });

            // GoalCardItem configurations
            modelBuilder.Entity<GoalCardItem>(entity =>
            {
                entity.HasKey(gci => gci.Id);
                entity.Property(gci => gci.Goal).IsRequired().HasMaxLength(500);
                entity.Property(gci => gci.Target80Percent).HasMaxLength(200);
                entity.Property(gci => gci.Target100Percent).HasMaxLength(200);
                entity.Property(gci => gci.Target120Percent).HasMaxLength(200);
                entity.Property(gci => gci.GoalDescription).HasMaxLength(1000);
                
                entity.HasOne(gci => gci.GoalCardTemplate)
                      .WithMany(gct => gct.Items)
                      .HasForeignKey(gci => gci.GoalCardTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(gci => gci.GoalType)
                      .WithMany(gt => gt.GoalCardItems)
                      .HasForeignKey(gci => gci.GoalTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // EmployeeGoalCard configurations
            modelBuilder.Entity<EmployeeGoalCard>(entity =>
            {
                entity.HasKey(egc => egc.Id);
                entity.Property(egc => egc.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(egc => egc.Employee)
                      .WithMany()
                      .HasForeignKey(egc => egc.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(egc => egc.GoalCardTemplate)
                      .WithMany(gct => gct.EmployeeGoalCards)
                      .HasForeignKey(egc => egc.GoalCardTemplateId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(egc => egc.CreatedByEmployee)
                      .WithMany()
                      .HasForeignKey(egc => egc.CreatedByEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Unique constraint: Aynı çalışan için aynı yılda aynı şablondan tek bir kart olabilir
                entity.HasIndex(egc => new { egc.EmployeeId, egc.GoalCardTemplateId, egc.Year })
                      .IsUnique();
            });

            // EmployeeGoalCardItem configurations
            modelBuilder.Entity<EmployeeGoalCardItem>(entity =>
            {
                entity.HasKey(egci => egci.Id);
                entity.Property(egci => egci.AchievementLevel).HasMaxLength(50);
                entity.Property(egci => egci.ManagerNotes).HasMaxLength(2000);
                entity.Property(egci => egci.EmployeeNotes).HasMaxLength(2000);
                
                entity.HasOne(egci => egci.EmployeeGoalCard)
                      .WithMany(egc => egc.Items)
                      .HasForeignKey(egci => egci.EmployeeGoalCardId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(egci => egci.GoalCardItem)
                      .WithMany(gci => gci.EmployeeGoalCardItems)
                      .HasForeignKey(egci => egci.GoalCardItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Holiday configurations
            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(200);
                entity.Property(h => h.Description).HasMaxLength(500);
                
                // Index for faster queries
                entity.HasIndex(h => new { h.Year, h.Date });
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


