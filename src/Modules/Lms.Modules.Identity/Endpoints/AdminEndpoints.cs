using Lms.Modules.Identity.Features.FindUsers;
using Lms.Modules.Identity.Features.GrantInstructor;
using Lms.Modules.Identity.Features.RevokeInstructor;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Http;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Pagination;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lms.Modules.Identity.Endpoints;

/// <summary>
/// <c>/api/admin</c> — curated instructor onboarding (artifacts/design/03-api-design.md §6).
/// </summary>
/// <remarks>
/// There is no admin console in the MVP; these are called with an HTTP client. The
/// <c>Admin</c> policy is applied at the group so a new endpoint added here is protected by
/// default rather than by whoever remembers to annotate it.
/// </remarks>
internal static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(AuthPolicies.Admin);

        group.MapGet("/users", async (
            string? search,
            PagingParams paging,
            IQueryHandler<FindUsersQuery, QueryResult<AdminUser>> handler,
            CancellationToken ct) =>
        {
            var page = paging.ToPageRequest();
            var result = await handler.HandleAsync(new FindUsersQuery(search, page), ct);

            return result.ToPagedHttpResult(page);
        })
        .WithName("FindUsers")
        .WithSummary("Find a user id by email")
        .Produces<PagedResult<AdminUser>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/users/{userId:guid}/grant-instructor", async (
            Guid userId,
            GrantInstructorRequest request,
            ICommandHandler<GrantInstructorCommand, InstructorGrant> handler,
            CancellationToken ct) =>
        {
            // The id comes from the route, not the body — one place to read it, and no way for
            // the two to disagree.
            var result = await handler.HandleAsync(
                new GrantInstructorCommand(userId, request.Slug, request.Headline), ct);

            return result.ToHttpResult();
        })
        .WithName("GrantInstructor")
        .WithSummary("Add the Instructor role and create an InstructorProfile")
        .Produces<InstructorGrant>(StatusCodes.Status200OK)
        // A malformed slug or headline is a bad request, so Error.Validation → 400. The 409 is
        // the slug already belonging to someone else.
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/users/{userId:guid}/revoke-instructor", async (
            Guid userId,
            ICommandHandler<RevokeInstructorCommand, Unit> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new RevokeInstructorCommand(userId), ct);
            return result.ToHttpResult();
        })
        .WithName("RevokeInstructor")
        .WithSummary("Remove the Instructor role; published courses are untouched")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}

public sealed record GrantInstructorRequest(string Slug, string Headline);
