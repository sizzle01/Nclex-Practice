using Microsoft.Extensions.Options;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Media;

/// <summary>
/// Media storage on local disk, for development and tests. Keys map to paths under the root
/// folder; a key that would lead outside it is refused.
/// </summary>
public sealed class LocalDiskMediaStorage(IOptions<MediaStorageOptions> options) : IMediaStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.LocalRoot);

    public async Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken)
    {
        var path = PathFor(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = PathFor(key);
        // Seekable, so the API can answer range requests (audio and video seek with them).
        Stream? stream = File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true) : null;
        return Task.FromResult(stream);
    }

    private string PathFor(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key));
        return path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? path
            : throw new ArgumentException($"Media key \"{key}\" leads outside the storage folder.", nameof(key));
    }
}
