using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Domain.Constants;

namespace SystemManagement.Infrastructure.Authentication;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => TryGetGuid(ClaimNames.UserId) ?? TryGetGuid(ClaimTypes.NameIdentifier);
    public string? Username => User?.FindFirstValue(ClaimNames.Username) ?? User?.Identity?.Name;
    public string? RoleCode => User?.FindFirstValue(ClaimNames.Role) ?? User?.FindFirstValue(ClaimTypes.Role);
    public int RoleLevel => int.TryParse(User?.FindFirstValue(ClaimNames.RoleLevel), out var level) ? level : 0;
    public Guid? DepartmentId => TryGetGuid(ClaimNames.DepartmentId);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public bool IsAdmin => RoleLevel >= RoleLevels.Admin || RoleCode == RoleCodes.Admin;

    private Guid? TryGetGuid(string claimType)
    {
        var value = User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
