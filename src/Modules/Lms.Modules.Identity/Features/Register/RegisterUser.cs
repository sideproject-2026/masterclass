using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Identity;

namespace Lms.Modules.Identity.Features.Register;

public sealed record RegisterUserCommand(string Email, string Password, string DisplayName)
    : ICommand<RegisteredUser>;

public sealed record RegisteredUser(Guid UserId, string Email, string DisplayName);

/// <summary>
/// Creates a student account. There is no path to self-register as an instructor —
/// that role is granted by an admin (artifacts/design/00-overview.md §5).
/// </summary>
internal sealed class RegisterUserHandler(
    UserManager<AppUser> users,
    IClock clock) : ICommandHandler<RegisterUserCommand, RegisteredUser>
{
    public async Task<Result<RegisteredUser>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var email = command.Email.Trim();

        if (await users.FindByEmailAsync(email) is not null)
        {
            return IdentityErrors.EmailAlreadyRegistered;
        }

        var user = AppUser.Create(email, command.DisplayName, clock.UtcNow);

        var created = await users.CreateAsync(user, command.Password);
        if (!created.Succeeded)
        {
            // Password-policy and email-format failures surface here. Safe to return: the
            // caller supplied these values, so nothing is disclosed about other accounts.
            return IdentityErrors.RegistrationFailed(Describe(created));
        }

        var roleAssigned = await users.AddToRoleAsync(user, Roles.Student);
        if (!roleAssigned.Succeeded)
        {
            // A user with no role can do nothing and would be invisible to every policy.
            // Better to leave no account than a broken one.
            await users.DeleteAsync(user);
            return IdentityErrors.RegistrationFailed(Describe(roleAssigned));
        }

        return new RegisteredUser(user.Id, email, user.DisplayName);
    }

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
