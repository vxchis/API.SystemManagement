namespace SystemManagement.Application.DTOs.Auth;

public sealed record LoginRequest(string Username, string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    CurrentUserDto User);

public sealed record CurrentUserDto(
    Guid Id,
    string Username,
    string FullName,
    string RoleCode,
    int RoleLevel,
    Guid? DepartmentId,
    string? DepartmentName,
    IReadOnlyCollection<Guid> DepartmentGroupIds);

public sealed record TokenUserInfo(
    Guid Id,
    string Username,
    string FullName,
    string RoleCode,
    int RoleLevel,
    Guid? DepartmentId,
    IReadOnlyCollection<Guid> DepartmentGroupIds);
