using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Auth;
using SystemManagement.Application.Services;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUser;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        var role = user.UserRoles.OrderByDescending(x => x.Role.Level).Select(x => x.Role).FirstOrDefault();
        if (role is null)
        {
            throw new UnauthorizedAccessException("Tài khoản chưa được gán vai trò.");
        }

        var departmentGroupIds = user.Employee?.DepartmentId is Guid departmentId
            ? await _dbContext.DepartmentGroupDepartments
                .Where(x => x.DepartmentId == departmentId && x.IsActive)
                .Select(x => x.DepartmentGroupId)
                .Distinct()
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        var tokenInfo = new TokenUserInfo(
            user.Id,
            user.Username,
            user.FullName,
            role.Code,
            role.Level,
            user.Employee?.DepartmentId,
            departmentGroupIds);

        var accessToken = _jwtTokenService.GenerateAccessToken(tokenInfo);
        var expiresAt = DateTime.UtcNow.AddHours(2);

        return new AuthResponse(accessToken, expiresAt, new CurrentUserDto(
            user.Id,
            user.Username,
            user.FullName,
            role.Code,
            role.Level,
            user.Employee?.DepartmentId,
            user.Employee?.Department.Name,
            departmentGroupIds));
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Chưa đăng nhập.");
        }

        var user = await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId.Value, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Không tìm thấy tài khoản.");
        }

        var role = user.UserRoles.OrderByDescending(x => x.Role.Level).Select(x => x.Role).First();
        var departmentGroupIds = user.Employee?.DepartmentId is Guid departmentId
            ? await _dbContext.DepartmentGroupDepartments
                .Where(x => x.DepartmentId == departmentId && x.IsActive)
                .Select(x => x.DepartmentGroupId)
                .Distinct()
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        return new CurrentUserDto(user.Id, user.Username, user.FullName, role.Code, role.Level, user.Employee?.DepartmentId, user.Employee?.Department.Name, departmentGroupIds);
    }
}
