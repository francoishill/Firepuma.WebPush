namespace Firepuma.WebPush.Abstractions.Models.ValueObjects;

public class SuccessOrFailure<TSuccess, TFailure> : Either<TSuccess, TFailure>
{
    public SuccessOrFailure(TSuccess success)
        : base(success)
    {
    }

    public SuccessOrFailure(TFailure failure)
        : base(failure)
    {
    }

    public TSuccess SuccessfulResult => Left;
    public TFailure Failure => Right;
    public bool IsSuccessful => IsLeft;

    public static implicit operator SuccessOrFailure<TSuccess, TFailure>(TSuccess left) => new(left);
    public static implicit operator SuccessOrFailure<TSuccess, TFailure>(TFailure right) => new(right);
}