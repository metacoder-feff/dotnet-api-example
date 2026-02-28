using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FEFF.Extentions.Jwt;

public class JwtFactory : IJwtFactory
{
//TODO: Freeze?
    private readonly JwtOptions _options;

    public JwtFactory(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateToken(IEnumerable<Claim> claims)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(claims);

        var secretKey = _options.GetKey();
        var sigOpts   = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var time = _options.TimeProvider ?? TimeProvider.System;
        var now = time.GetUtcNow();
        var expires = now.Add(_options.TokenLifeTime);

        var opts = new JwtSecurityToken(
            issuer              : _options.Issuer,
            audience            : _options.Audience,
            claims              : claims,
            expires             : expires.DateTime,
            signingCredentials  : sigOpts
        );

        return new JwtSecurityTokenHandler().WriteToken(opts);
    }
}