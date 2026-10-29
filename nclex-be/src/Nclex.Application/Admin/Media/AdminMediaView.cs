namespace Nclex.Application.Admin.Media;

/// <summary>A media asset as the editor shows it: what it is, and what it still needs before publishing.</summary>
/// <param name="Uploaded">False until the file has been PUT to the upload link.</param>
/// <param name="HasCaptions">Whether WebVTT captions are stored.</param>
/// <param name="HasTranscript">Whether a transcript is stored.</param>
public sealed record AdminMediaView(
    Guid Id,
    string Kind,
    string MimeType,
    long Bytes,
    string? AltText,
    bool Uploaded,
    bool HasCaptions,
    bool HasTranscript);
