namespace SystemManagement.Application.Common.Interfaces;

public interface IDepartmentAccessService
{
    Task<bool> CanAccessDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetVisibleDepartmentIdsAsync(CancellationToken cancellationToken = default);
}
