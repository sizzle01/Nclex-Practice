using Nclex.Application.Content.ImportQuestions;

namespace Nclex.Infrastructure.Content;

/// <summary>Reads the files beside an import file from its folder; paths may not leave that folder.</summary>
public sealed class DirectoryContentFiles(string folder) : IContentFiles
{
    private readonly string _root = Path.GetFullPath(folder);

    public async Task<byte[]?> ReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (Path.IsPathRooted(relativePath))
        {
            return null;
        }

        var path = Path.GetFullPath(Path.Combine(_root, relativePath));
        var inside = path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal);
        return inside && File.Exists(path) ? await File.ReadAllBytesAsync(path, cancellationToken) : null;
    }
}
