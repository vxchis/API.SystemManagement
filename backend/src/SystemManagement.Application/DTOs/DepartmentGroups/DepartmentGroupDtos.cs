using SystemManagement.Application.DTOs.Departments;

namespace SystemManagement.Application.DTOs.DepartmentGroups;

public sealed record DepartmentGroupDto(Guid Id, string Code, string Name, string? Description, bool IsActive, IReadOnlyCollection<DepartmentDto> Departments);
public sealed record CreateDepartmentGroupRequest(string Code, string Name, string? Description);
public sealed record AddDepartmentToGroupRequest(Guid DepartmentId);
