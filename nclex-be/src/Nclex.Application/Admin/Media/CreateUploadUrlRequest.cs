namespace Nclex.Application.Admin.Media;

/// <summary>Body of POST /admin/media/upload-url (api.md section 8).</summary>
/// <param name="MimeType">The file's type, e.g. "image/png"; see MediaUploadRules for the allowed list.</param>
/// <param name="Bytes">The file's exact size; the upload must match it.</param>
public sealed record CreateUploadUrlRequest(string MimeType, long Bytes);
