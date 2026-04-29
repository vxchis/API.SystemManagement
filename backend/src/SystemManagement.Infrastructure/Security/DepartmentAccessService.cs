using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Security;

public sealed class DepartmentAccessService : IDepartmentAccessService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public DepartmentAccessService(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<bool> CanAccessDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsAdmin)
        {
            return true;
        }

        if (!_currentUser.DepartmentId.HasValue)
        {
            return false;
        }

        if (_currentUser.DepartmentId.Value == departmentId)
        {
            return true;
        }

        return await _dbContext.DepartmentGroupDepartments
            .Where(x => x.IsActive && x.DepartmentId == _currentUser.DepartmentId.Value)
            .Join(_dbContext.DepartmentGroupDepartments.Where(x => x.IsActive && x.DepartmentId == departmentId),
                left => left.DepartmentGroupId,
                right => right.DepartmentGroupId,
                (left, right) => left.DepartmentGroupId)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetVisibleDepartmentIdsAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsAdmin)
        {
            return await _dbContext.Departments
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        if (!_currentUser.DepartmentId.HasValue)
        {
            return Array.Empty<Guid>();
        }

        var currentDepartmentId = _currentUser.DepartmentId.Value;

        var groupIds = await _dbContext.DepartmentGroupDepartments
            .Where(x => x.IsActive && x.DepartmentId == currentDepartmentId)
            .Select(x => x.DepartmentGroupId)
            .ToListAsync(cancellationToken);

        var departmentIds = await _dbContext.DepartmentGroupDepartments
            .Where(x => x.IsActive && groupIds.Contains(x.DepartmentGroupId))
            .Select(x => x.DepartmentId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!departmentIds.Contains(currentDepartmentId))
        {
            departmentIds.Add(currentDepartmentId);
        }

        return departmentIds;
    }
}
