using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lms.Modules.Identity.Infrastructure;

/// <summary>
/// Stops the committed development signing key from reaching a real environment.
/// </summary>
/// <remarks>
/// A weak or shared signing key means anyone holding it can mint a token for any user with
/// any role — it is a total authentication bypass, and a silent one. The dev key is committed
/// so <c>dotnet run</c> works with no setup; this class is the reason that is safe.
/// Runs at startup via <c>ValidateOnStart()</c>.
/// </remarks>
internal sealed class JwtOptionsValidator(IHostEnvironment environment) : IValidateOptions<JwtOptions>
{
    internal const string DevelopmentKey = "DEVELOPMENT-ONLY-DO-NOT-USE-IN-PRODUCTION-8f3c1a5e";

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (environment.IsDevelopment())
        {
            return ValidateOptionsResult.Success;
        }

        if (string.Equals(options.SigningKey, DevelopmentKey, StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Fail(
                $"Jwt:SigningKey is still the committed development key in the "
                + $"'{environment.EnvironmentName}' environment. Supply a real key from Key Vault. "
                + "Anyone with this key can mint a token for any user in any role.");
        }

        return ValidateOptionsResult.Success;
    }
}
