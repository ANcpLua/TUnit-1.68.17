using TUnit.Assertions.Exceptions;

namespace TUnit.Patterns.Assertions;

public abstract class PaymentProvider;

public sealed class CardProvider : PaymentProvider;

public sealed class InvoiceProvider : PaymentProvider;

/// <summary>
/// Assert assignability of the represented types, as needed when discovering plugins.
/// </summary>
public sealed class TypeAssertionTests
{
    [Test]
    public async Task Generic_assignability_evaluates_the_represented_type()
    {
        await Assert.That(typeof(CardProvider)).IsAssignableTo<PaymentProvider>();
        await Assert.That(typeof(PaymentProvider)).IsAssignableFrom<CardProvider>();
        await Assert.That(typeof(CardProvider)).IsNotAssignableTo<InvoiceProvider>();
        await Assert.That(typeof(InvoiceProvider)).IsNotAssignableFrom<CardProvider>();
    }

    // Plugin discovery hands over Type values, not type arguments: the Type overloads cover that case.
    [Test]
    public async Task Runtime_type_overloads_take_a_type_argument()
    {
        Type discovered = typeof(InvoiceProvider);

        await Assert.That(discovered).IsAssignableTo(typeof(PaymentProvider));
        await Assert.That(typeof(PaymentProvider)).IsAssignableFrom(discovered);
    }

    [Test]
    public async Task Failure_names_the_represented_types()
    {
        var exception = await Assert.That(async () => await Assert.That(typeof(CardProvider)).IsAssignableTo<InvoiceProvider>())
            .Throws<AssertionException>();

        await Assert.That(exception!.Message).Contains(nameof(InvoiceProvider));
    }

    // Documented limit: represented-type semantics only apply to the first assertion after Assert.That(type).
    // Behind .And the check falls back to the runtime type of the Type object, so a valid pair fails.
    [Test]
    public async Task Chained_assignability_inspects_the_runtime_type()
    {
        await Assert.That(async () => await Assert.That(typeof(CardProvider))
                .IsAssignableTo<PaymentProvider>()
                .And.IsAssignableTo<PaymentProvider>())
            .Throws<AssertionException>();

        await Assert.That(typeof(CardProvider))
            .IsAssignableTo<PaymentProvider>()
            .And.IsAssignableTo<Type>();
    }
}
