namespace JaTakTilbud.Core.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have error.");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, string.Empty);

    public static Result Failure(string error) => new Result(false, error);
}


public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    protected Result(T value) : base(true, string.Empty)
    {
        _value = value;
    }

    protected Result(string error) : base(false, error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new Result<T>(value);

    public static new Result<T> Failure(string error) => new Result<T>(error);
}