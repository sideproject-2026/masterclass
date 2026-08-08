namespace Lms.SharedKernel.Results;

/// <summary>
/// The response type for a command that returns nothing.
/// <c>Result&lt;Unit&gt;</c> rather than a non-generic result keeps every handler
/// signature identical, so the pipeline decorators need only one shape.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value => default;

    public override string ToString() => "()";
}
