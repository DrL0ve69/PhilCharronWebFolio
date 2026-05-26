namespace PhilCharronWebFolio.Application.Common.Results;

public record Error(string Code, string Description);

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = Array.Empty<Error>();
    }

    private Result(IReadOnlyList<Error> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new([error]);
    public static Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);

    public T UnWrap() => IsSuccess ? Value! : throw new InvalidOperationException("Cannot unwrap a failure result.");
}
