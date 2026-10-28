using Nclex.Application.Attempts.Reveal;

namespace Nclex.Application.Attempts.SubmitItem;

/// <summary>Body returned by POST /attempts/{id}/items/{position}/submit.</summary>
public sealed record SubmitItemResult(ItemReveal Reveal);
