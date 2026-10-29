namespace Nclex.Application.Admin.Media;

/// <summary>Where to PUT the file, like a presigned S3 URL (api.md section 8).</summary>
/// <param name="MediaId">The new asset's id, to use as mediaId in exhibits and options.</param>
/// <param name="UploadUrl">A relative, signed link; PUT the file there with its Content-Type, no token needed.</param>
/// <param name="ExpiresAt">When the link stops working.</param>
public sealed record UploadUrlResponse(Guid MediaId, string UploadUrl, DateTimeOffset ExpiresAt);
