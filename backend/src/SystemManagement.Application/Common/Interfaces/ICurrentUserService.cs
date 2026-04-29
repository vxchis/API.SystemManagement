namespace SystemManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? RoleCode { get; }
    int RoleLevel { get; }
    Guid? DepartmentId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
