namespace SystemManagement.Application.DTOs.Users;

public sealed record RoleDto(Guid Id, string Code, string Name, int Level);
public sealed record UserDto(Guid Id, string Username, string FullName, string? Email, string? PhoneNumber, bool IsActive, string RoleCode, int RoleLevel, Guid? DepartmentId, string? DepartmentName);
public sealed record CreateUserRequest(string Username, string Password, string FullName, string? Email, string? PhoneNumber, string RoleCode, Guid? DepartmentId, Guid? PositionId, Guid? ManagerEmployeeId);
