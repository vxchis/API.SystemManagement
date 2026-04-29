using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Domain.Common;
using SystemManagement.Domain.Entities;

namespace SystemManagement.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentGroup> DepartmentGroups => Set<DepartmentGroup>();
    public DbSet<DepartmentGroupDepartment> DepartmentGroupDepartments => Set<DepartmentGroupDepartment>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TaskProgressLog> TaskProgressLogs => Set<TaskProgressLog>();
    public DbSet<TaskFile> TaskFiles => Set<TaskFile>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.Username).HasMaxLength(100).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Role>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(x => new { x.UserId, x.RoleId });
            b.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
            b.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<Department>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<DepartmentGroup>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<DepartmentGroupDepartment>(b =>
        {
            b.HasKey(x => new { x.DepartmentGroupId, x.DepartmentId });
            b.HasOne(x => x.DepartmentGroup).WithMany(x => x.Departments).HasForeignKey(x => x.DepartmentGroupId);
            b.HasOne(x => x.Department).WithMany(x => x.GroupDepartments).HasForeignKey(x => x.DepartmentId);
        });

        modelBuilder.Entity<Position>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Employee>(b =>
        {
            b.HasIndex(x => x.EmployeeCode).IsUnique();
            b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            b.HasOne(x => x.Department).WithMany(x => x.Employees).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Position).WithMany(x => x.Employees).HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.User).WithOne(x => x.Employee).HasForeignKey<Employee>(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.ManagerEmployee)
                .WithMany(x => x.DirectReports)
                .HasForeignKey(x => x.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<TaskItem>(b =>
        {
            b.HasIndex(x => x.TaskCode).IsUnique();
            b.Property(x => x.TaskCode).HasMaxLength(50).IsRequired();
            b.Property(x => x.Title).HasMaxLength(300).IsRequired();
            b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskProgressLog>(b =>
        {
            b.HasOne(x => x.TaskItem).WithMany(x => x.ProgressLogs).HasForeignKey(x => x.TaskItemId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ActionByUser).WithMany().HasForeignKey(x => x.ActionByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskFile>(b =>
        {
            b.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            b.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            b.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.TaskItemId, x.AttachmentType });
            b.HasIndex(x => x.TaskProgressLogId);
            b.HasOne(x => x.TaskItem).WithMany(x => x.Files).HasForeignKey(x => x.TaskItemId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.TaskProgressLog).WithMany(x => x.Files).HasForeignKey(x => x.TaskProgressLogId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
            b.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.Property(x => x.Type).HasMaxLength(100).IsRequired();
            b.Property(x => x.Title).HasMaxLength(300).IsRequired();
            b.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            b.Property(x => x.RelatedEntityType).HasMaxLength(100);
            b.HasOne(x => x.TargetUser).WithMany().HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TargetUserId, x.IsRead, x.CreatedAt });
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService?.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
