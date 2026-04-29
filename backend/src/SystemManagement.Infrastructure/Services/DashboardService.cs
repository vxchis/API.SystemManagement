using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Dashboard;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Constants;
using SystemManagement.Domain.Enums;
using SystemManagement.Infrastructure.Persistence;
using TaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDepartmentAccessService _departmentAccess;

    public DashboardService(AppDbContext dbContext, ICurrentUserService currentUser, IDepartmentAccessService departmentAccess)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _departmentAccess = departmentAccess;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Chưa đăng nhập.");
        var visibleDepartmentIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);

        var query = _dbContext.TaskItems.Where(x => !x.IsDeleted);
        if (_currentUser.RoleLevel <= RoleLevels.ChuyenVien)
        {
            query = query.Where(x => x.AssignedToUserId == userId || x.AssignedByUserId == userId);
        }
        else
        {
            query = query.Where(x => visibleDepartmentIds.Contains(x.DepartmentId));
        }

        var total = await query.CountAsync(cancellationToken);
        var my = await query.CountAsync(x => x.AssignedToUserId == userId, cancellationToken);
        var completed = await query.CountAsync(x => x.Status == TaskStatus.Completed, cancellationToken);
        var overdue = await query.CountAsync(x => x.Status != TaskStatus.Completed && x.Status != TaskStatus.Cancelled && x.DueDate < DateTime.UtcNow, cancellationToken);

        return new DashboardSummaryDto(total, my, completed, overdue, visibleDepartmentIds.Count);
    }
}
