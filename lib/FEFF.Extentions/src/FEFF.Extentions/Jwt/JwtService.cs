using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FEFF.Extentions.Jwt;

public class SymmetricJwtService : IConfigureNamedOptions<JwtBearerOptions>, IJwtFactory
{
    private readonly JwtOptions _options;

    // JWT handler support for overriding timeProvider does not work correctly
    //private readonly TimeProvider _timeProvider;

    public SymmetricJwtService(IOptions<JwtOptions> options/*, TimeProvider timeProvider*/)
    {
        _options        = options.Value;
        //_timeProvider   = timeProvider;
        
        var vr = JwtOptionsValidator.Validate(_options);
        if (vr.Failed)
            throw new ArgumentException($"Invalid 'options': {vr.FailureMessage}");
    }

    // for IConfigureNamedOptions
    // see https://github.com/dotnet/aspnetcore/issues/50274
    // name = nameof(AuthenticateScheme) for multiple schemes
    public void Configure(string? name, JwtBearerOptions o)
    {
        Configure(o);
    }

    // for IConfigureOptions
    public void Configure(JwtBearerOptions o)
    {
        //TODO: disable optionally
        ArgumentException.ThrowIfNullOrEmpty(_options.Issuer);
        ArgumentException.ThrowIfNullOrEmpty(_options.Audience);

        o.TokenValidationParameters = new()
        {
            ValidateIssuer          = true,
            ValidateAudience        = true,
            ValidateLifetime        = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer             = _options.Issuer,
            ValidAudience           = _options.Audience,
            IssuerSigningKey        = _options.GetKey(),
        };
        // JWT handler support for overriding timeProvider does not work correctly
        //o.TimeProvider = _timeProvider;
    }

    public string CreateToken(IEnumerable<Claim> claims)
    {
        ThrowHelper.Argument.ThrowIfNullOrEmpty(claims);

        var secretKey = _options.GetKey();
        var sigOpts   = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        //var now = _timeProvider.GetUtcNow();
        var now = TimeProvider.System.GetUtcNow();
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