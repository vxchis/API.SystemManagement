using SystemManagement.Application.DTOs.Auth;

namespace SystemManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(TokenUserInfo userInfo);
}
