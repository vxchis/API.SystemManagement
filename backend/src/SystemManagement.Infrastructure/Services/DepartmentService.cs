using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Departments;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Entities;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _dbContext;
    private readonly IDepartmentAccessService _departmentAccess;

    public DepartmentService(AppDbContext dbContext, IDepartmentAccessService departmentAccess)
    {
        _dbContext = dbContext;
        _departmentAccess = departmentAccess;
    }

    public async Task<IReadOnlyCollection<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var visibleIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);
        var departments = await _dbContext.Departments
            .Where(x => !x.IsDeleted && visibleIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        return departments.Select(x => x.ToDto()).ToList();
    }

    public async Task<DepartmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _departmentAccess.CanAccessDepartmentAsync(id, cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem phòng ban này.");
        }

        var department = await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phòng ban.");
        return department.ToDto();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = new Department { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description };
        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return department.ToDto();
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy phòng ban.");
        department.Code = request.Code.Trim();
        department.Name = request.Name.Trim();
        department.Description = request.Description;
        department.IsActive = request.IsActive;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return department.ToDto();
    }
}
