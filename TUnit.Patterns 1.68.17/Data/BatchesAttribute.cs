namespace TUnit.Patterns.Data;

/// <summary>Untyped generator: rows are object?[] so the element types are only known at run time.</summary>
public sealed class BatchesAttribute : UntypedDataSourceGeneratorAttribute
{
    public const string Label = "batch";

    protected override IEnumerable<Func<object?[]?>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () => [Label, new int[] { 1, 2, 3 }];
    }
}
