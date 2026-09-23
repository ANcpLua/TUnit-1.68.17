namespace TUnit.Patterns.Data;

/// <summary>Controls how a <see cref="Sum"/> argument is rendered in the display name.</summary>
public sealed class SumFormatter : ArgumentDisplayFormatter
{
    private const string Format = "{0}+{1}";

    public override bool CanHandle(object? value) => value is Sum;

    public override string FormatValue(object? value)
    {
        var sum = (Sum)value!;
        return string.Format(null, Format, sum.Left, sum.Right);
    }
}
