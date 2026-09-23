namespace TUnit.Patterns.Data;

public sealed class TestDataRowTests
{
    private const string AdminName = "Admin login";
    private const string GuestName = "Guest login";
    private const string EmptyName = "Empty credentials";
    private const string NotImplemented = "Not implemented yet";
    private const string Admin = "admin";
    private const string Guest = "guest";
    private const string Secret = "secret123";
    private const string Security = "Security";

    public static IEnumerable<TestDataRow<(string Username, string Password, string ExpectedName)>> Credentials()
    {
        yield return new((Admin, Secret, AdminName), DisplayName: AdminName, Categories: [Security]);
        yield return new((Guest, Guest, GuestName), DisplayName: GuestName);
        yield return new((string.Empty, string.Empty, EmptyName), DisplayName: EmptyName, Skip: NotImplemented);
    }

    [Test]
    [MethodDataSource(nameof(Credentials))]
    public async Task Row_metadata_drives_display_name_and_skip(string username, string password, string expectedName)
    {
        await Assert.That(TestContext.Current!.Metadata.DisplayName).IsEqualTo(expectedName);
        await Assert.That(username).IsNotEmpty();
        await Assert.That(password).IsNotEmpty();
    }
}

public sealed class DisplayNameTests
{
    private const string Template = "$sum equals $expected";
    private const string Expected = "1+2 equals 3";

    public static IEnumerable<Func<(Sum, int)>> Sums()
    {
        yield return () => (new Sum(1, 2), 3);
    }

    [Test]
    [MethodDataSource(nameof(Sums))]
    [ArgumentDisplayFormatter<SumFormatter>]
    [DisplayName(Template)]
    public async Task Formatter_and_template_compose(Sum sum, int expected)
    {
        await Assert.That(sum.Total).IsEqualTo(expected);
        await Assert.That(TestContext.Current!.Metadata.DisplayName).IsEqualTo(Expected);
    }
}

public sealed class CombinedDataSourceTests
{
    private const string Utf8 = "utf-8";
    private const string Ascii = "ascii";

    public static IEnumerable<string> Encodings()
    {
        yield return Utf8;
        yield return Ascii;
    }

    private static readonly List<(int, string, bool)> Seen = [];

    // 2 x 2 x 2 = 8 cases from three independent sources.
    [Test]
    [CombinedDataSources]
    public async Task Cartesian_product_of_mixed_sources(
        [Arguments(1, 2)] int page,
        [MethodDataSource(nameof(Encodings))] string encoding,
        [Arguments(true, false)] bool compressed)
    {
        lock (Seen)
        {
            Seen.Add((page, encoding, compressed));
        }

        await Assert.That(page).IsIn([1, 2]);
        await Assert.That(encoding).IsIn([Utf8, Ascii]);
    }
}

public sealed class DeferredEnumerationTests
{
    private const int Cases = 100;

    public static IEnumerable<int> ManyCases() => Enumerable.Range(0, Cases);

    // One node at discovery, one nested result per row at run time.
    [Test]
    [MethodDataSource(nameof(ManyCases), DeferEnumeration = true)]
    public async Task Discovery_sees_one_node(int input) => await Assert.That(input).IsBetween(0, Cases - 1);
}

public sealed class AsyncGeneratorTests
{
    private const int Count = 10;
    private const long TenthFibonacci = 34;

    [Test]
    [Fibonacci(Count)]
    public async Task Async_generator_yields_typed_tuples(int index, long value)
    {
        await Assert.That(index).IsBetween(0, Count - 1);

        if (index == Count - 1)
        {
            await Assert.That(value).IsEqualTo(TenthFibonacci);
        }
    }
}

public sealed class UntypedGeneratorTests
{
    [Test]
    [Batches]
    public async Task Untyped_source_supplies_a_label_and_array(string label, int[] values)
    {
        await Assert.That(label).IsEqualTo(BatchesAttribute.Label);
        await Assert.That(values).IsEquivalentTo([1, 2, 3]);
    }
}
