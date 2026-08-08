using System.Text.RegularExpressions;

namespace Lms.ArchitectureTests;

/// <summary>
/// Paging is implemented once, in <c>SharedKernel.Persistence.ToQueryResultAsync</c>
/// (artifacts/design/09-code-conventions.md §8.3).
/// </summary>
/// <remarks>
/// NetArchTest works on types, and this is a rule about call sites — so it is a source scan.
/// Slightly unusual for a test, but the alternative is a rule enforced only by memory, and
/// hand-rolled paging is exactly the kind of thing that gets copied into the second query and
/// then the fifth, each one forgetting the ordering requirement.
/// </remarks>
public partial class PagingConventionTests
{
    private const string AllowedProject = "Lms.SharedKernel.Persistence";

    [Fact]
    public void Skip_and_Take_appear_only_in_SharedKernel_Persistence()
    {
        var offenders = SourceFiles()
            .Where(file => !file.FullName.Contains(AllowedProject, StringComparison.Ordinal))
            .Select(file => new { file, hits = PagingCallSites(File.ReadAllText(file.FullName)) })
            .Where(x => x.hits.Count > 0)
            .Select(x => $"{Relative(x.file)} — {string.Join(", ", x.hits)}")
            .ToList();

        offenders.ShouldBeEmpty(
            $"Skip(/Take( must appear only in {AllowedProject}.ToQueryResultAsync. "
            + "Paging without a uniquely-ordered query is non-deterministic in PostgreSQL: "
            + "rows repeat across pages or vanish entirely."
            + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void The_scan_actually_finds_source_files()
    {
        // Without this, a broken path would make the rule above pass silently forever.
        SourceFiles().Count.ShouldBeGreaterThan(20, "the source scan found almost nothing — "
            + "the repository root was probably not located correctly");
    }

    private static List<FileInfo> SourceFiles()
    {
        var src = new DirectoryInfo(Path.Combine(RepositoryRoot(), "src"));

        return src.EnumerateFiles("*.cs", SearchOption.AllDirectories)
            .Where(f => !f.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => !f.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => !f.FullName.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();
    }

    private static List<string> PagingCallSites(string source) =>
        PagingCall().Matches(source).Select(m => m.Value).Distinct().ToList();

    private static string Relative(FileInfo file) =>
        Path.GetRelativePath(RepositoryRoot(), file.FullName);

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, ".git")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from " + AppContext.BaseDirectory);
    }

    // Word boundary so ToListAsync, Skipped, Taken and similar do not match.
    [GeneratedRegex(@"\b(Skip|Take)\s*\(", RegexOptions.Compiled)]
    private static partial Regex PagingCall();
}
