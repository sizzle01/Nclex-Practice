namespace Nclex.Application.Abstractions;

/// <summary>
/// Where question media files live (architecture.md section 4). Local development stores them on
/// disk; S3 with a CDN replaces that in deployed environments behind the same interface.
/// </summary>
public interface IMediaStorage
{
    /// <summary>Stores a file under a key, replacing any file already there.</summary>
    Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken);

    /// <summary>Opens a stored file for reading, or returns null when there is none.</summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);
}
