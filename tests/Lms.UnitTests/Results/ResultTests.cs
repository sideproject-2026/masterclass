using Lms.SharedKernel.Results;

namespace Lms.UnitTests.Results;

public class ResultTests
{
    private static readonly Error SampleError = Error.NotFound("course.not_found", "No such course.");

    [Fact]
    public void Success_carries_its_value()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void An_error_converts_implicitly_to_a_failed_result()
    {
        Result<int> result = SampleError;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Reading_the_value_of_a_failure_throws()
    {
        Result<int> result = SampleError;

        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void TryGetValue_reports_success_and_yields_the_value()
    {
        Result<string> result = "ok";

        result.TryGetValue(out var value).ShouldBeTrue();
        value.ShouldBe("ok");
    }

    [Fact]
    public void TryGetValue_reports_failure()
    {
        Result<string> result = SampleError;

        result.TryGetValue(out _).ShouldBeFalse();
    }

    [Fact]
    public void Match_collapses_both_branches()
    {
        Result<int> success = 7;
        Result<int> failure = SampleError;

        success.Match(v => $"ok:{v}", e => $"err:{e.Code}").ShouldBe("ok:7");
        failure.Match(v => $"ok:{v}", e => $"err:{e.Code}").ShouldBe("err:course.not_found");
    }

    [Fact]
    public void FirstFailureOr_returns_the_first_failure()
    {
        var result = Result.FirstFailureOr(
            Result.Success(),
            Result.Failure(SampleError),
            Result.Failure(Error.Conflict("other", "Other.")));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("course.not_found");
    }

    [Fact]
    public void FirstFailureOr_succeeds_when_all_succeed()
    {
        Result.FirstFailureOr(Result.Success(), Result.Success()).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void AsResult_discards_the_value_but_keeps_the_error()
    {
        Result<int> failure = SampleError;

        var widened = failure.AsResult();

        widened.IsFailure.ShouldBeTrue();
        widened.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Error_None_is_recognisable()
    {
        Error.None.IsNone.ShouldBeTrue();
        SampleError.IsNone.ShouldBeFalse();
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Invariant)]
    public void Error_factories_set_the_matching_type(ErrorType type)
    {
        var error = type switch
        {
            ErrorType.Validation => Error.Validation("c", "m"),
            ErrorType.Forbidden => Error.Forbidden("c", "m"),
            ErrorType.NotFound => Error.NotFound("c", "m"),
            ErrorType.Conflict => Error.Conflict("c", "m"),
            ErrorType.Invariant => Error.Invariant("c", "m"),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        error.Type.ShouldBe(type);
    }
}
