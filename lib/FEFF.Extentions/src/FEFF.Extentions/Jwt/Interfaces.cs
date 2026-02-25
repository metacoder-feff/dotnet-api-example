using System.Security.Claims;

namespace FEFF.Extentions.Jwt;

public interface IJwtFactory
{
    string CreateToken(IEnumerable<Claim> claims);
}