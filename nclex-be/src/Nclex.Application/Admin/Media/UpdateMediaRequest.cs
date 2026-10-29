namespace Nclex.Application.Admin.Media;

/// <summary>Body of PATCH /admin/media/{id}. Fields left out stay as they are.</summary>
/// <param name="AltText">The image's text alternative (required for images before publishing).</param>
/// <param name="Captions">WebVTT captions as text, starting with "WEBVTT" (audio and video).</param>
/// <param name="Transcript">A plain-text transcript (audio and video).</param>
public sealed record UpdateMediaRequest(string? AltText, string? Captions, string? Transcript);
