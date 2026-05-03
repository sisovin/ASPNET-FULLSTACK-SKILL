using PhoneShop.Shared.DTOs;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Services;

public interface ITokenService
{
    AuthResponse GenerateToken(ApplicationUser user);
}
