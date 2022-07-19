namespace Firepuma.WebPush.Abstractions.Models.ValueObjects;

// https://stackoverflow.com/a/34636692/1224216
public class Either<TLeft, TRight>
{
    private readonly TLeft _left;
    private readonly TRight _right;
    private readonly bool _isLeft;

    public Either(TLeft left)
    {
        _left = left;
        _isLeft = true;
    }

    public Either(TRight right)
    {
        _right = right;
        _isLeft = false;
    }


    public TLeft Left => _left;
    public TRight Right => _right;
    public bool IsLeft => _isLeft;
    
    public T Match<T>(Func<TLeft, T> leftFunc, Func<TRight, T> rightFunc) => _isLeft ? leftFunc(_left) : rightFunc(_right);
    
    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);
}