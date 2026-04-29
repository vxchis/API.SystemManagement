using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Domain.Constants;
using SystemManagement.Domain.Entities;

namespace SystemManagement.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(db);
        await SeedDepartmentsAsync(db);
        await SeedPositionsAsync(db);
        await SeedDefaultUsersAsync(db, passwordHasher);
        await SeedDepartmentGroupsAsync(db);
    }

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        var roles = new[]
        {
            new Role { Code = RoleCodes.Admin, Name = "Quản trị hệ thống", Level = RoleLevels.Admin },
            new Role { Code = RoleCodes.TruongPhong, Name = "Trưởng phòng", Level = RoleLevels.TruongPhong },
            new Role { Code = RoleCodes.PhoPhong, Name = "Phó trưởng phòng", Level = RoleLevels.PhoPhong },
            new Role { Code = RoleCodes.ChuyenVien, Name = "Chuyên viên", Level = RoleLevels.ChuyenVien }
        };

        foreach (var role in roles)
        {
            if (!await db.Roles.AnyAsync(x => x.Code == role.Code))
            {
                db.Roles.Add(role);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(AppDbContext db)
    {
        var departments = new[]
        {
            new Department { Code = "VH-XH", Name = "Phòng Văn hóa - Xã hội" },
            new Department { Code = "NOI-VU", Name = "Nhóm Nội vụ" },
            new Department { Code = "GD-YT-AS", Name = "Nhóm Giáo dục - Y tế - An sinh" },
            new Department { Code = "VH-TT-DL", Name = "Nhóm Văn hóa - Thể thao - Du lịch" },
            new Department { Code = "KHCN-CDS", Name = "Nhóm Khoa học - Công nghệ - Chuyển đổi số" },
            new Department { Code = "HC-TC", Name = "Nhóm Hành chính - Tài chính" }
        };

        foreach (var department in departments)
        {
            if (!await db.Departments.AnyAsync(x => x.Code == department.Code))
            {
                db.Departments.Add(department);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedPositionsAsync(AppDbContext db)
    {
        var positions = new[]
        {
            new Position { Code = "TP", Name = "Trưởng phòng" },
            new Position { Code = "PTP", Name = "Phó trưởng phòng" },
            new Position { Code = "CV", Name = "Chuyên viên" }
        };

        foreach (var position in positions)
        {
            if (!await db.Positions.AnyAsync(x => x.Code == position.Code))
            {
                db.Positions.Add(position);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedDefaultUsersAsync(AppDbContext db, IPasswordHasher passwordHasher)
    {
        var adminRole = await db.Roles.SingleAsync(x => x.Code == RoleCodes.Admin);
        var truongPhongRole = await db.Roles.SingleAsync(x => x.Code == RoleCodes.TruongPhong);
        var phoPhongRole = await db.Roles.SingleAsync(x => x.Code == RoleCodes.PhoPhong);
        var chuyenVienRole = await db.Roles.SingleAsync(x => x.Code == RoleCodes.ChuyenVien);

        var departmentMain = await db.Departments.SingleAsync(x => x.Code == "VH-XH");
        var departmentNoiVu = await db.Departments.SingleAsync(x => x.Code == "NOI-VU");
        var departmentGd = await db.Departments.SingleAsync(x => x.Code == "GD-YT-AS");

        var positionTp = await db.Positions.SingleAsync(x => x.Code == "TP");
        var positionPtp = await db.Positions.SingleAsync(x => x.Code == "PTP");
        var positionCv = await db.Positions.SingleAsync(x => x.Code == "CV");

        await EnsureUserWithEmployeeAsync(db, passwordHasher, "admin", "Admin@123", "System Administrator", adminRole, departmentMain, positionTp, null);
        var truongPhongEmployee = await EnsureUserWithEmployeeAsync(db, passwordHasher, "truongphong", "Admin@123", "Trưởng phòng mẫu", truongPhongRole, departmentMain, positionTp, null);
        var phoPhongEmployee = await EnsureUserWithEmployeeAsync(db, passwordHasher, "phophong", "Admin@123", "Phó phòng mẫu", phoPhongRole, departmentNoiVu, positionPtp, truongPhongEmployee.Id);
        await EnsureUserWithEmployeeAsync(db, passwordHasher, "chuyenvien1", "Admin@123", "Chuyên viên 1", chuyenVienRole, departmentNoiVu, positionCv, phoPhongEmployee.Id);
        await EnsureUserWithEmployeeAsync(db, passwordHasher, "chuyenvien2", "Admin@123", "Chuyên viên 2", chuyenVienRole, departmentGd, positionCv, phoPhongEmployee.Id);
    }

    private static async Task<Employee> EnsureUserWithEmployeeAsync(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        string username,
        string passwordValue,
        string fullName,
        Role role,
        Department department,
        Position position,
        Guid? managerEmployeeId)
    {
        var existingUser = await db.Users
            .Include(x => x.Employee)
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Username == username);

        if (existingUser is not null && existingUser.Employee is not null)
        {
            return existingUser.Employee;
        }

        var password = passwordHasher.HashPassword(passwordValue);
        var user = existingUser ?? new User
        {
            Username = username,
            FullName = fullName,
            Email = $"{username}@system.local",
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            IsActive = true
        };

        if (!user.UserRoles.Any(x => x.RoleId == role.Id))
        {
            user.UserRoles.Add(new UserRole { User = user, Role = role });
        }

        var employee = new Employee
        {
            EmployeeCode = $"EMP-{username.ToUpperInvariant()}",
            FullName = fullName,
            Department = department,
            Position = position,
            User = user,
            Email = user.Email,
            IsActive = true,
            ManagerEmployeeId = managerEmployeeId
        };

        if (existingUser is null)
        {
            db.Users.Add(user);
        }

        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }

    private static async Task SeedDepartmentGroupsAsync(AppDbContext db)
    {
        var group = await db.DepartmentGroups.FirstOrDefaultAsync(x => x.Code == "NHOM-AN-SINH");
        if (group is null)
        {
            group = new DepartmentGroup
            {
                Code = "NHOM-AN-SINH",
                Name = "Nhóm Nội vụ - Giáo dục Y tế",
                Description = "Nhóm phòng ban dùng để chia sẻ dữ liệu quản lý"
            };
            db.DepartmentGroups.Add(group);
            await db.SaveChangesAsync();
        }

        var deptCodes = new[] { "NOI-VU", "GD-YT-AS" };
        var departments = await db.Departments.Where(x => deptCodes.Contains(x.Code)).ToListAsync();
        foreach (var department in departments)
        {
            var exists = await db.DepartmentGroupDepartments.AnyAsync(x => x.DepartmentGroupId == group.Id && x.DepartmentId == department.Id);
            if (!exists)
            {
                db.DepartmentGroupDepartments.Add(new DepartmentGroupDepartment
                {
                    DepartmentGroupId = group.Id,
                    DepartmentId = department.Id,
                    IsActive = true
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
