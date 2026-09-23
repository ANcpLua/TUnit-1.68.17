namespace AdvancedPatterns.Tests.Security;

public sealed class SafeFileStoreTests
{
    [Test]
    public async Task ReadTextAsync_RejectsParentTraversalOutsideRoot(CancellationToken cancellationToken)
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        await File.WriteAllTextAsync(
            workspace.OutsideFile,
            "SECRET_OUTSIDE_ROOT",
            cancellationToken);

        SafeFileStore store = new(workspace.Root);

        await Assert.That(async () => { _ = await store.ReadTextAsync("../outside.txt", cancellationToken); })
            .Throws<ArgumentException>()
            .WithMessageContaining("outside the file-store root");
    }

    [Test]
    public async Task SearchAsync_DoesNotLeakFilesOutsideRoot(CancellationToken cancellationToken)
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        await File.WriteAllTextAsync(
            Path.Combine(workspace.Root, "visible.txt"),
            "VISIBLE_CONTENT",
            cancellationToken);
        await File.WriteAllTextAsync(
            workspace.OutsideFile,
            "SECRET_OUTSIDE_ROOT",
            cancellationToken);

        SafeFileStore store = new(workspace.Root);

        IReadOnlyList<string> matches = await store.SearchAsync(
            "SECRET_OUTSIDE_ROOT",
            cancellationToken);

        await Assert.That(matches).IsEmpty();
    }

    [Test]
    public async Task WriteTextAsync_CreatesNestedFileInsideRoot(CancellationToken cancellationToken)
    {
        using TempWorkspace workspace = TempWorkspace.Create();
        SafeFileStore store = new(workspace.Root);

        await store.WriteTextAsync("notes/today.txt", "inside", cancellationToken);

        await Assert.That(
            await File.ReadAllTextAsync(
                Path.Combine(workspace.Root, "notes", "today.txt"),
                cancellationToken))
            .IsEqualTo("inside");
    }
}

internal sealed class SafeFileStore
{
    private readonly string _root;

    public SafeFileStore(string root)
    {
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> ReadTextAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        string path = GetSafePath(relativePath);
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task WriteTextAsync(
        string relativePath,
        string text,
        CancellationToken cancellationToken = default)
    {
        string path = GetSafePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> SearchAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        List<string> matches = [];
        foreach (string path in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            string fullPath = GetSafePath(Path.GetRelativePath(_root, path));
            string content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            if (content.Contains(text, StringComparison.Ordinal))
            {
                matches.Add(Path.GetRelativePath(_root, fullPath));
            }
        }

        return matches;
    }

    private string GetSafePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Path must be relative to the file-store root.", nameof(relativePath));
        }

        string fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        string rootWithSeparator = _root.EndsWith(Path.DirectorySeparatorChar)
            ? _root
            : _root + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException("Path resolves outside the file-store root.", nameof(relativePath));
        }

        return fullPath;
    }
}

internal sealed class TempWorkspace : IDisposable
{
    private TempWorkspace(string parent, string root, string outsideFile)
    {
        Parent = parent;
        Root = root;
        OutsideFile = outsideFile;
    }

    public string Parent { get; }

    public string Root { get; }

    public string OutsideFile { get; }

    public static TempWorkspace Create()
    {
        string parent = Path.Combine(Path.GetTempPath(), "advanced-patterns", Guid.NewGuid().ToString("N"));
        string root = Path.Combine(parent, "root");
        string outsideFile = Path.Combine(parent, "outside.txt");

        Directory.CreateDirectory(root);
        return new TempWorkspace(parent, root, outsideFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(Parent))
        {
            Directory.Delete(Parent, recursive: true);
        }
    }
}
