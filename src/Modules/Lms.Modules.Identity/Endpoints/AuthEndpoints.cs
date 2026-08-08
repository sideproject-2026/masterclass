using Lms.Modules.Identity.Features.Login;
using Lms.Modules.Identity.Features.Logout;
using Lms.Modules.Identity.Features.Refresh;
using Lms.Modules.Identity.Features.Register;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Http;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lms.Modules.Identity.Endpoints;

/// <summary>
/// <c>/api/auth</c> — consumed by the TanStack Start BFF, not by the browser
/// (artifacts/design/03-api-design.md §3).
/// </summary>
internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .AllowAnonymous()
            // Account lockout caps attempts per account; this caps them per caller, which is
            // what blunts credential stuffing spread across many accounts.
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/register", async (
            RegisterRequest request,
            ICommandHandler<RegisterUserCommand, RegisteredUser> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new RegisterUserCommand(request.Email, request.Password, request.DisplayName), ct);

            return result.ToCreatedResult(user => $"/api/users/{user.UserId}");
        })
        .WithName("Register")
        .WithSummary("Create a student account")
        .Produces<RegisteredUser>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (
            LoginRequest request,
            ICommandHandler<LoginUserCommand, AuthTokens> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new LoginUserCommand(request.Email, request.Password), ct);

            return result.ToHttpResult();
        })
        .WithName("Login")
        .WithSummary("Exchange credentials for tokens")
        .Produces<AuthTokens>(StatusCodes.Status200OK)
        // Deliberately one failure shape: an unknown email and a wrong password are
        // indistinguishable, so this endpoint is not a user-enumeration oracle.
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (
            RefreshRequest request,
            ICommandHandler<RefreshSessionCommand, AuthTokens> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new RefreshSessionCommand(request.RefreshToken), ct);
            return result.ToHttpResult();
        })
        .WithName("Refresh")
        .WithSummary("Rotate the token pair")
        .Produces<AuthTokens>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", async (
            RefreshRequest request,
            ICommandHandler<LogoutCommand, Unit> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new LogoutCommand(request.RefreshToken), ct);
            return result.ToHttpResult();
        })
        .WithName("Logout")
        .WithSummary("Revoke the refresh token")
        // Succeeds even for an unknown token: logout must not reveal whether one exists.
        .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);
