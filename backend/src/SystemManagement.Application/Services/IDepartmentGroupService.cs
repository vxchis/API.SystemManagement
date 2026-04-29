using SystemManagement.Application.DTOs.DepartmentGroups;

namespace SystemManagement.Application.Services;

public interface IDepartmentGroupService
{
    Task<IReadOnlyCollection<DepartmentGroupDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DepartmentGroupDto> CreateAsync(CreateDepartmentGroupRequest request, CancellationToken cancellationToken);
    Task AddDepartmentAsync(Guid groupId, AddDepartmentToGroupRequest request, CancellationToken cancellationToken);
    Task RemoveDepartmentAsync(Guid groupId, Guid departmentId, CancellationToken cancellationToken);
}
