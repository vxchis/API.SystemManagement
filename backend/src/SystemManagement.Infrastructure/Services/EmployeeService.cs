using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Employees;
using SystemManagement.Application.Services;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _dbContext;
    private readonly IDepartmentAccessService _departmentAccess;
    private readonly ICurrentUserService _currentUser;

    public EmployeeService(AppDbContext dbContext, IDepartmentAccessService departmentAccess, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _departmentAccess = departmentAccess;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var visibleIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);
        var employees = await BuildQuery()
            .Where(x => !x.IsDeleted && visibleIds.Contains(x.DepartmentId))
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
        return employees.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<EmployeeDto>> GetSubordinatesAsync(CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;
        if (!currentUserId.HasValue)
        {
            return Array.Empty<EmployeeDto>();
        }

        var currentEmployeeId = await _dbContext.Employees
            .Where(x => !x.IsDeleted && x.UserId == currentUserId.Value)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentEmployeeId.HasValue)
        {
            return Array.Empty<EmployeeDto>();
        }

        var employees = await BuildQuery()
            .Where(x => !x.IsDeleted && x.ManagerEmployeeId == currentEmployeeId.Value)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
        return employees.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken)
    {
        var positions = await _dbContext.Positions.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return positions.Select(x => x.ToDto()).ToList();
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (!await _departmentAccess.CanAccessDepartmentAsync(request.DepartmentId, cancellationToken) && !_currentUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền tạo nhân sự cho phòng ban này.");
        }

        if (request.ManagerEmployeeId.HasValue)
        {
            var manager = await _dbContext.Employees.FirstOrDefaultAsync(x => x.Id == request.ManagerEmployeeId.Value && !x.IsDeleted, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy quản lý trực tiếp.");

            if (!await _departmentAccess.CanAccessDepartmentAsync(manager.DepartmentId, cancellationToken) && !_currentUser.IsAdmin)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền gán quản lý trực tiếp này.");
            }
        }

        var employee = new SystemManagement.Domain.Entities.Employee
        {
            EmployeeCode = request.EmployeeCode.Trim(),
            FullName = request.FullName.Trim(),
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            UserId = request.UserId,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ManagerEmployeeId = request.ManagerEmployeeId,
            IsActive = true
        };
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var created = await BuildQuery().FirstAsync(x => x.Id == employee.Id, cancellationToken);
        return created.ToDto();
    }

    private IQueryable<SystemManagement.Domain.Entities.Employee> BuildQuery() => _dbContext.Employees
        .Include(x => x.Department)
        .Include(x => x.Position)
        .Include(x => x.ManagerEmployee);
}
