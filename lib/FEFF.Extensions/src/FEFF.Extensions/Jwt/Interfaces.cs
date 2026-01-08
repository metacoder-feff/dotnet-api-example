using System.Security.Claims;

namespace FEFF.Extensions.Jwt;

public interface IJwtFactory
{
    string CreateToken(IEnumerable<Claim> claims);
}