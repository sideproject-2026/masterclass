using NetArchTest.Rules;

namespace Lms.ArchitectureTests;

/// <summary>
/// The rules from artifacts/design/01-architecture.md §4, made mechanical.
/// </summary>
/// <remarks>
/// Most of these pass vacuously today — Catalog, Enrollment, Identity and Media are still
/// stubs. That is deliberate: retrofitting boundary rules onto code that already violates
/// them is how a modular monolith quietly stops being modular.
/// </remarks>
public class ModuleBoundaryTests
{
    // Rule 1 — the highest-value rule here. Lms.SharedKernel.Persistence and
    // Lms.SharedKernel.Http exist only so that SharedKernel itself stays clean; if this
    // ever goes red, those two projects have lost their purpose.

    [Fact]
    public void SharedKernel_does_not_depend_on_EntityFrameworkCore()
    {
        var result = Types.InAssembly(Architecture.SharedKernel)
            .ShouldNot()
            .HaveDependencyOnAny(Architecture.EntityFrameworkNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            "Lms.SharedKernel must stay persistence-free. EF Core belongs in "
            + "Lms.SharedKernel.Persistence — otherwise *.Contracts projects, which reference "
            + "SharedKernel, transitively drag EF Core into plain DTO assemblies."));
    }

    [Fact]
    public void SharedKernel_does_not_depend_on_AspNetCore()
    {
        var result = Types.InAssembly(Architecture.SharedKernel)
            .ShouldNot()
            .HaveDependencyOnAny(Architecture.AspNetCoreNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            "Lms.SharedKernel must stay framework-free. HTTP concerns belong in "
            + "Lms.SharedKernel.Http."));
    }

    // Rule 2 — Contracts are plain DTOs plus query interfaces. SharedKernel is a leaf, so
    // allowing that one reference keeps the Catalog<->Enrollment contract edges acyclic.

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    public void Contracts_do_not_depend_on_infrastructure(string module)
    {
        var assembly = Architecture.ContractAssemblies[module];

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                Architecture.EntityFrameworkNamespace,
                Architecture.AspNetCoreNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"Lms.Modules.{module}.Contracts must hold only DTOs, query interfaces and events."));
    }

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    public void Contracts_do_not_depend_on_any_other_module(string module)
    {
        var assembly = Architecture.ContractAssemblies[module];
        var foreignModules = Architecture.Modules.Keys
            .Where(m => m != module)
            .Select(m => $"Lms.Modules.{m}")
            .ToArray();

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(foreignModules)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"Lms.Modules.{module}.Contracts must reference only Lms.SharedKernel. "
            + "A Contracts project that reaches into another module reintroduces the cycle "
            + "the split exists to prevent."));
    }

    // Rules 3 and 4 — the domain is plain C#. Persistence and transport live at the edges.
    //
    // ResideInNamespace matches the namespace and everything beneath it, so this also covers
    // Domain/Events and friends. Note: the package's XML docs advertise
    // ResideInNamespaceStartingWith on Predicate, but the shipped assembly does not expose it —
    // the doc file is out of sync. Use ResideInNamespace.

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    [InlineData("Media")]
    [InlineData("Notifications")]
    public void Domain_does_not_depend_on_EntityFrameworkCore(string module)
    {
        var result = Types.InAssembly(Architecture.Modules[module])
            .That().ResideInNamespace($"Lms.Modules.{module}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(Architecture.EntityFrameworkNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"{module}.Domain must be persistence-ignorant. Mapping belongs in "
            + "Infrastructure/Configurations via IEntityTypeConfiguration<T>."));
    }

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    [InlineData("Media")]
    [InlineData("Notifications")]
    public void Domain_does_not_depend_on_Http(string module)
    {
        var result = Types.InAssembly(Architecture.Modules[module])
            .That().ResideInNamespace($"Lms.Modules.{module}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(Architecture.HttpNamespaces)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"{module}.Domain must not know about HTTP. Endpoints live in Endpoints/ and Features/."));
    }

    /// <summary>
    /// ASP.NET Core Identity is an implementation detail of the Identity module.
    /// </summary>
    /// <remarks>
    /// Encodes 04-adr-authentication.md §5 rule 2 — "<c>IdentityUser</c> never leaves the
    /// Identity Module". Other modules see <c>Identity.Contracts.UserSummary</c> and a
    /// <c>UserId</c>. This is what keeps a future swap to OpenIddict or a managed provider
    /// a change in one module rather than a change everywhere.
    /// </remarks>
    [Theory]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    [InlineData("Media")]
    [InlineData("Notifications")]
    public void Only_the_Identity_module_knows_about_AspNetCore_Identity(string module)
    {
        var result = Types.InAssembly(Architecture.Modules[module])
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.AspNetCore.Identity")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"Lms.Modules.{module} must not reference ASP.NET Core Identity. Users cross a "
            + "module boundary as a UserId and a Contracts DTO, never as an IdentityUser."));
    }

    // Rule 5 — the rule the whole modular monolith rests on.

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    [InlineData("Media")]
    [InlineData("Notifications")]
    public void A_module_never_reaches_inside_another_module(string module)
    {
        var forbidden = Architecture.Modules.Keys
            .Where(other => other != module)
            .SelectMany(other => new[]
            {
                $"Lms.Modules.{other}.Domain",
                $"Lms.Modules.{other}.Features",
                $"Lms.Modules.{other}.Infrastructure",
                $"Lms.Modules.{other}.Endpoints"
            })
            .ToArray();

        var result = Types.InAssembly(Architecture.Modules[module])
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"Lms.Modules.{module} may reference another module only through its *.Contracts. "
            + "Reaching into Domain, Features or Infrastructure couples the two modules and "
            + "makes the boundary fictional."));
    }

    // Rule 6 — 01-architecture.md §7.3: Aspire is dev-time orchestration and must not
    // become a production dependency. The reference runs one way: AppHost -> Api.

    [Theory]
    [InlineData("Identity")]
    [InlineData("Catalog")]
    [InlineData("Enrollment")]
    [InlineData("Media")]
    [InlineData("Notifications")]
    public void Modules_do_not_depend_on_Aspire(string module)
    {
        var result = Types.InAssembly(Architecture.Modules[module])
            .ShouldNot()
            .HaveDependencyOnAny(Architecture.AspireNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.Explain(
            $"Lms.Modules.{module} must run without an AppHost. Aspire is local orchestration; "
            + "the deployed system is plain containers."));
    }

    // Rule 7 — vacuous today; guards every slice from S-1 onward.

    [Fact]
    public void Command_and_query_handlers_are_internal_and_sealed()
    {
        foreach (var (module, assembly) in Architecture.Modules)
        {
            var result = Types.InAssembly(assembly)
                .That().ImplementInterface(typeof(SharedKernel.Messaging.ICommandHandler<,>))
                .Or().ImplementInterface(typeof(SharedKernel.Messaging.IQueryHandler<,>))
                .Should().BeSealed()
                .And().NotBePublic()
                .GetResult();

            result.IsSuccessful.ShouldBeTrue(result.Explain(
                $"Handlers in {module} must be internal sealed — nothing outside the module "
                + "resolves them (09-code-conventions.md §9)."));
        }
    }
}
