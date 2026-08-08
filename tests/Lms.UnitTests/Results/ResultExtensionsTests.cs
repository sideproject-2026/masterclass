using Lms.SharedKernel.Results;

namespace Lms.UnitTests.Results;

public class ResultExtensionsTests
{
    private static readonly Error SampleError = Error.Invariant("course.empty", "Course has no chapters.");

    [Fact]
    public void Map_transforms_a_success()
    {
        Result<int> result = 21;

        result.Map(v => v * 2).Value.ShouldBe(42);
    }

    [Fact]
    public void Map_short_circuits_a_failure()
    {
        Result<int> result = SampleError;
        var mapCalled = false;

        var mapped = result.Map(v => { mapCalled = true; return v * 2; });

        mapCalled.ShouldBeFalse();
        mapped.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Bind_chains_an_operation_that_can_fail()
    {
        Result<int> result = 4;

        result.Bind(v => v > 0 ? Result<string>.Success("positive") : SampleError)
              .Value.ShouldBe("positive");
    }

    [Fact]
    public void Bind_propagates_the_original_failure()
    {
        Result<int> result = SampleError;

        result.Bind(_ => Result<string>.Success("never")).Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Tap_runs_on_success_and_preserves_the_value()
    {
        Result<int> result = 5;
        var seen = 0;

        var returned = result.Tap(v => seen = v);

        seen.ShouldBe(5);
        returned.Value.ShouldBe(5);
    }

    [Fact]
    public void Tap_does_not_run_on_failure()
    {
        Result<int> result = SampleError;
        var ran = false;

        result.Tap(_ => ran = true);

        ran.ShouldBeFalse();
    }

    [Fact]
    public void Ensure_fails_when_the_predicate_does_not_hold()
    {
        Result<int> result = 3;

        result.Ensure(v => v > 10, SampleError).Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Ensure_passes_through_when_the_predicate_holds()
    {
        Result<int> result = 30;

        result.Ensure(v => v > 10, SampleError).Value.ShouldBe(30);
    }

    [Fact]
    public async Task TapAsync_awaits_the_side_effect_on_success()
    {
        Result<int> result = 9;
        var saved = false;

        await result.TapAsync(async _ => { await Task.Yield(); saved = true; });

        saved.ShouldBeTrue();
    }

    [Fact]
    public async Task TapAsync_skips_the_side_effect_on_failure()
    {
        Result<int> result = SampleError;
        var saved = false;

        var returned = await result.TapAsync(async _ => { await Task.Yield(); saved = true; });

        saved.ShouldBeFalse();
        returned.IsFailure.ShouldBeTrue();
    }
}
