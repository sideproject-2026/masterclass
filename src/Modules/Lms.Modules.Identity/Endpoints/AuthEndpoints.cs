using Lms.Modules.Identity.Features.Login;
using Lms.Modules.Identity.Features.Register;
using Lms.SharedKernel.Http;
using Lms.SharedKernel.Messaging;
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
            .AllowAnonymous();

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

        return app;
    }
}

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);
