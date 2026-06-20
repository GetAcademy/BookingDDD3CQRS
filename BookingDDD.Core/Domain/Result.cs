namespace BookingDDD.Core.Domain;

public sealed class Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public T? Value { get; }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Fail(string errorMessage) => new(errorMessage);
}
