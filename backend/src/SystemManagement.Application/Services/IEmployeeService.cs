using SystemManagement.Application.DTOs.Employees;

namespace SystemManagement.Application.Services;

public interface IEmployeeService
{
    Task<IReadOnlyCollection<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmployeeDto>> GetSubordinatesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken);
}
