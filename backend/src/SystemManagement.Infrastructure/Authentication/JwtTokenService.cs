using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Auth;
using SystemManagement.Domain.Constants;

namespace SystemManagement.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(TokenUserInfo userInfo)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userInfo.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userInfo.Username),
            new(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new(ClaimTypes.Name, userInfo.Username),
            new(ClaimTypes.Role, userInfo.RoleCode),
            new(ClaimNames.UserId, userInfo.Id.ToString()),
            new(ClaimNames.Username, userInfo.Username),
            new(ClaimNames.FullName, userInfo.FullName),
            new(ClaimNames.Role, userInfo.RoleCode),
            new(ClaimNames.RoleLevel, userInfo.RoleLevel.ToString())
        };

        if (userInfo.DepartmentId.HasValue)
        {
            claims.Add(new Claim(ClaimNames.DepartmentId, userInfo.DepartmentId.Value.ToString()));
        }

        foreach (var groupId in userInfo.DepartmentGroupIds)
        {
            claims.Add(new Claim(ClaimNames.DepartmentGroupId, groupId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
