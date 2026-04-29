namespace SystemManagement.Application.DTOs.Departments;

public sealed record DepartmentDto(Guid Id, string Code, string Name, string? Description, bool IsActive);
public sealed record CreateDepartmentRequest(string Code, string Name, string? Description);
public sealed record UpdateDepartmentRequest(string Code, string Name, string? Description, bool IsActive);
