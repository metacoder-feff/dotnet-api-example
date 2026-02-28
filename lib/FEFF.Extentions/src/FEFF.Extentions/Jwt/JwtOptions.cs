using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FEFF.Extentions.Jwt;

public class JwtOptions
{
    // to create KeyedHashAlgorithm for algorithm 'HS256', the key size must be greater than: '256' bits
    public required string   SecretKey      { get; set; }
    public          string?  Issuer         { get; set; }
    public          string?  Audience       { get; set; }
    public required TimeSpan TokenLifeTime  { get; set; }

    // for testing
    public TimeProvider? TimeProvider { get; set; }

    public byte[] GetKeyBytes()
    {
        return Encoding.UTF8.GetBytes(SecretKey);
    }

    public SymmetricSecurityKey GetKey()
    {
        var keyBytes = GetKeyBytes();
        return new SymmetricSecurityKey(keyBytes);
    }
}

public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        return Validate(options);
    }

    public static ValidateOptionsResult Validate(JwtOptions options)
    {
        if (options.SecretKey.IsNullOrEmpty())
            return ValidateOptionsResult.Fail("Jwt: SecretKey.IsNullOrEmpty()");

        // early error signalisation
        // for: JwtMiddleware: JwtBearerOptions.TokenValidationParameters.IssuerSigningKey <= _options.GetKey()
        var size = options.GetKeyBytes().Length * 8;
        if( size < 256) // min 32 bytes
            return ValidateOptionsResult.Fail($"Jwt: The encryption algorithm 'HS256' requires a key size of at least '256' bits. Key is of size: '{size}'.");

        if (options.TokenLifeTime <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Jwt: options.TokenLifeTime <= TimeSpan.Zero or not defined");

        return ValidateOptionsResult.Success;
    }
}