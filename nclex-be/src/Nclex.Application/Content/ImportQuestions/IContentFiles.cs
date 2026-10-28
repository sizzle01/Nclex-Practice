namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// The files that sit beside an import file, such as its media and captions. Paths in the file are
/// relative to the file's folder.
/// </summary>
public interface IContentFiles
{
    /// <summary>The file's bytes, or null when it does not exist or the path leaves the folder.</summary>
    Task<byte[]?> ReadAsync(string relativePath, CancellationToken cancellationToken);
}

/// <summary>For imports with no folder of their own: every file is missing.</summary>
public sealed class NoContentFiles : IContentFiles
{
    public static readonly NoContentFiles Instance = new();

    public Task<byte[]?> ReadAsync(string relativePath, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);
}
