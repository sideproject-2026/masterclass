using System.Reflection;
using Lms.SharedKernel.Results;

// Both NetArchTest and Xunit define TestResult.
using TestResult = NetArchTest.Rules.TestResult;

namespace Lms.ArchitectureTests;

/// <summary>
/// Shared assembly handles and the failure formatter every rule uses.
/// </summary>
internal static class Architecture
{
    public const string EntityFrameworkNamespace = "Microsoft.EntityFrameworkCore";
    public const string AspNetCoreNamespace = "Microsoft.AspNetCore";
    public const string AspireNamespace = "Aspire";

    public static Assembly SharedKernel => typeof(Result).Assembly;

    public static Assembly SharedKernelPersistence =>
        typeof(SharedKernel.Persistence.QueryableExtensions).Assembly;

    public static Assembly SharedKernelHttp => typeof(SharedKernel.Http.HttpResults).Assembly;

    /// <summary>Every module assembly, keyed by module name.</summary>
    public static IReadOnlyDictionary<string, Assembly> Modules { get; } =
        new Dictionary<string, Assembly>
        {
            ["Identity"] = typeof(Modules.Identity.IdentityModule).Assembly,
            ["Catalog"] = typeof(Modules.Catalog.CatalogModule).Assembly,
            ["Enrollment"] = typeof(Modules.Enrollment.EnrollmentModule).Assembly,
            ["Media"] = typeof(Modules.Media.MediaModule).Assembly,
            ["Notifications"] = typeof(Modules.Notifications.NotificationsModule).Assembly
        };

    /// <summary>Contracts assemblies. Media and Notifications expose none.</summary>
    public static IReadOnlyDictionary<string, Assembly> ContractAssemblies { get; } =
        new Dictionary<string, Assembly>
        {
            ["Identity"] = typeof(Modules.Identity.Contracts.IIdentityContractsMarker).Assembly,
            ["Catalog"] = typeof(Modules.Catalog.Contracts.ICatalogContractsMarker).Assembly,
            ["Enrollment"] = typeof(Modules.Enrollment.Contracts.IEnrollmentContractsMarker).Assembly
        };

    /// <summary>
    /// Names the offending types, because "the architecture test failed" is not actionable
    /// and a guardrail you cannot act on gets deleted.
    /// </summary>
    public static string Explain(this TestResult result, string rule)
    {
        if (result.IsSuccessful)
        {
            return string.Empty;
        }

        var offenders = (result.FailingTypes ?? [])
            .Select(t => string.IsNullOrWhiteSpace(t.Explanation)
                ? t.FullName
                : $"{t.FullName}  ({t.Explanation})");

        return $"{rule}{Environment.NewLine}Offending types:{Environment.NewLine}  - "
            + string.Join($"{Environment.NewLine}  - ", offenders);
    }
}
