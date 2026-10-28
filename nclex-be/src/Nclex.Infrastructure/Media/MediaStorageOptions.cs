namespace Nclex.Infrastructure.Media;

/// <summary>Settings for storing media and signing links to it, bound from the "MediaStorage" section.</summary>
public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    /// <summary>The HMAC-SHA256 key needs at least 256 bits, as for access tokens.</summary>
    public const int MinimumSigningKeyBytes = 32;

    /// <summary>
    /// Folder for media files on local disk. A relative path is resolved from the working folder,
    /// which dotnet run sets to the project folder, so the API and the worker share "../../.local/media".
    /// </summary>
    public string LocalRoot { get; set; } = string.Empty;

    /// <summary>Secret key for signed media links. Comes from secrets in deployed environments.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>How long a signed link works: long enough to load and play a clip, short enough not to share.</summary>
    public TimeSpan LinkLifetime { get; set; } = TimeSpan.FromMinutes(10);
}
