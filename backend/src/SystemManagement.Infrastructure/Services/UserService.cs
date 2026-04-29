using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Users;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Entities;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDepartmentAccessService _departmentAccess;
    private readonly ICurrentUserService _currentUser;

    public UserService(AppDbContext dbContext, IPasswordHasher passwordHasher, IDepartmentAccessService departmentAccess, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _departmentAccess = departmentAccess;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var visibleIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);
        var users = await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .Where(x => !x.IsDeleted && (x.Employee == null || visibleIds.Contains(x.Employee.DepartmentId)))
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
        return users.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles.OrderByDescending(x => x.Level).ToListAsync(cancellationToken);
        return roles.Select(x => x.ToDto()).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Username == request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username đã tồn tại.");
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Code == request.RoleCode, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy vai trò.");

        if (!_currentUser.IsAdmin && role.Level >= _currentUser.RoleLevel)
        {
            throw new UnauthorizedAccessException("Chỉ được tạo tài khoản có vai trò thấp hơn.");
        }

        if (request.DepartmentId.HasValue && !await _departmentAccess.CanAccessDepartmentAsync(request.DepartmentId.Value, cancellationToken) && !_currentUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền tạo tài khoản cho phòng ban này.");
        }

        var password = _passwordHasher.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            IsActive = true
        };
        user.UserRoles.Add(new UserRole { User = user, Role = role });
        _dbContext.Users.Add(user);

        if (request.DepartmentId.HasValue && request.PositionId.HasValue)
        {
            var employee = new Employee
            {
                EmployeeCode = $"EMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                FullName = request.FullName.Trim(),
                DepartmentId = request.DepartmentId.Value,
                PositionId = request.PositionId.Value,
                User = user,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                ManagerEmployeeId = request.ManagerEmployeeId,
                IsActive = true
            };
            _dbContext.Employees.Add(employee);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        var createdUser = await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .FirstAsync(x => x.Id == user.Id, cancellationToken);
        return createdUser.ToDto();
    }
}
