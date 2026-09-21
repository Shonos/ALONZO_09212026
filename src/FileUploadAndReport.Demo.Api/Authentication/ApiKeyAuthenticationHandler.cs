using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FileUploadAndReport.Demo.Api.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredApiKey = _configuration["FILE_UPLOAD_API_KEY"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("The API key is not configured."));
        }

        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out var providedApiKey)
            || string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var configuredBytes = Encoding.UTF8.GetBytes(configuredApiKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedApiKey.ToString());

        if (!CryptographicOperations.FixedTimeEquals(configuredBytes, providedBytes))
        {
            return Task.FromResult(AuthenticateResult.Fail("The API key is invalid."));
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "api-key-client") },
            ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        return Task.CompletedTask;
    }
}
