using SystemManagement.Application.DTOs.Departments;

namespace SystemManagement.Application.Services;

public interface IDepartmentService
{
    Task<IReadOnlyCollection<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DepartmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken);
    Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken);
}
