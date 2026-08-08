using System.Security.Claims;
using Lms.Modules.Identity.Features.Me;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Http;
using Lms.SharedKernel.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lms.Modules.Identity.Endpoints;

/// <summary>
/// <c>/api/me</c> — the caller's own account (artifacts/design/03-api-design.md §3).
/// </summary>
internal static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        // Authenticated only. Any registered user holds Student, so this policy is simply
        // "we know who you are".
        var group = app.MapGroup("/api/me")
            .WithTags("Me")
            .RequireAuthorization(AuthPolicies.Student);

        group.MapGet("/", async (
            ClaimsPrincipal caller,
            IQueryHandler<GetCurrentUserQuery, CurrentUserDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetCurrentUserQuery(caller.GetUserId()), ct);
            return result.ToHttpResult();
        })
        .WithName("GetCurrentUser")
        .WithSummary("The signed-in user and their roles")
        .Produces<CurrentUserDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/", async (
            UpdateMeRequest request,
            ClaimsPrincipal caller,
            ICommandHandler<UpdateCurrentUserCommand, CurrentUserDto> handler,
            CancellationToken ct) =>
        {
            // The id comes from the token, never the body — otherwise this endpoint would
            // let any caller rename any account.
            var result = await handler.HandleAsync(
                new UpdateCurrentUserCommand(caller.GetUserId(), request.DisplayName), ct);

            return result.ToHttpResult();
        })
        .WithName("UpdateCurrentUser")
        .Produces<CurrentUserDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}

public sealed record UpdateMeRequest(string DisplayName);
