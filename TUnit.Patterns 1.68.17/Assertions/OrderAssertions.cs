using System.ComponentModel;
using TUnit.Assertions.Attributes;
using TUnit.Assertions.Core;

namespace TUnit.Patterns.Assertions;

public sealed record Order(string Id, decimal Total, IReadOnlyList<string> Lines);

/// <summary>[GenerateAssertion] turns plain predicates into chainable Assert.That(...) extensions with And/Or support.</summary>
public static partial class OrderAssertions
{
    public const string EmptyOrder = "order has no lines";
    private const string NegativeTotalFormat = "total {0} is negative";

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GenerateAssertion(ExpectationMessage = "to be payable")]
    public static AssertionResult IsPayable(this Order order)
    {
        if (order.Lines.Count == 0)
        {
            return AssertionResult.Failed(EmptyOrder);
        }

        return order.Total < 0
            ? AssertionResult.Failed(string.Format(null, NegativeTotalFormat, order.Total))
            : AssertionResult.Passed;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [GenerateAssertion(ExpectationMessage = "to have at least {count} line(s)")]
    public static bool HasAtLeastLines(this Order order, int count) => order.Lines.Count >= count;
}

public static class Prefixes
{
    public static bool HasPrefix(string value, string prefix) => value.StartsWith(prefix, StringComparison.Ordinal);
}

/// <summary>
/// [AssertionFrom] lifts an existing static method into an assertion, here once as-is and once negated.
/// (Verified on 1.68.17: {parameter} placeholders are only substituted for [GenerateAssertion]; keep these messages literal.)
/// </summary>
[AssertionFrom<string>(typeof(Prefixes), nameof(Prefixes.HasPrefix), ExpectationMessage = "to start with the prefix")]
[AssertionFrom<string>(typeof(Prefixes), nameof(Prefixes.HasPrefix), CustomName = "LacksPrefix", NegateLogic = true, ExpectationMessage = "to start with the prefix")]
public static partial class PrefixAssertions
{
}
