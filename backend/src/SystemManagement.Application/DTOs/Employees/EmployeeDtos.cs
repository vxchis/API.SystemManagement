namespace SystemManagement.Application.DTOs.Employees;

public sealed record PositionDto(Guid Id, string Code, string Name, bool IsActive);

public sealed record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionName,
    Guid? UserId,
    string? Email,
    string? PhoneNumber,
    Guid? ManagerEmployeeId,
    string? ManagerEmployeeName,
    bool IsActive);

public sealed record CreateEmployeeRequest(
    string EmployeeCode,
    string FullName,
    Guid DepartmentId,
    Guid PositionId,
    Guid? UserId,
    string? Email,
    string? PhoneNumber,
    Guid? ManagerEmployeeId);
