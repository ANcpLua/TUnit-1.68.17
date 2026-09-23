namespace TUnit.Patterns.Data;

public sealed record Sum(int Left, int Right)
{
    public int Total => Left + Right;
}
