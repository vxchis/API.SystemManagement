using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.DepartmentGroups;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Entities;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class DepartmentGroupService : IDepartmentGroupService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public DepartmentGroupService(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<DepartmentGroupDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.DepartmentGroups
            .Include(x => x.Departments).ThenInclude(x => x.Department)
            .Where(x => !x.IsDeleted && x.IsActive);

        if (!_currentUser.IsAdmin && _currentUser.DepartmentId.HasValue)
        {
            var currentDepartmentId = _currentUser.DepartmentId.Value;
            query = query.Where(g => g.Departments.Any(d => d.DepartmentId == currentDepartmentId && d.IsActive));
        }

        var groups = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return groups.Select(x => x.ToDto()).ToList();
    }

    public async Task<DepartmentGroupDto> CreateAsync(CreateDepartmentGroupRequest request, CancellationToken cancellationToken)
    {
        var group = new DepartmentGroup { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description };
        _dbContext.DepartmentGroups.Add(group);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return group.ToDto();
    }

    public async Task AddDepartmentAsync(Guid groupId, AddDepartmentToGroupRequest request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.DepartmentGroupDepartments.AnyAsync(x => x.DepartmentGroupId == groupId && x.DepartmentId == request.DepartmentId, cancellationToken);
        if (!exists)
        {
            _dbContext.DepartmentGroupDepartments.Add(new DepartmentGroupDepartment { DepartmentGroupId = groupId, DepartmentId = request.DepartmentId, IsActive = true });
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveDepartmentAsync(Guid groupId, Guid departmentId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.DepartmentGroupDepartments.FirstOrDefaultAsync(x => x.DepartmentGroupId == groupId && x.DepartmentId == departmentId, cancellationToken);
        if (entity is not null)
        {
            _dbContext.DepartmentGroupDepartments.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
