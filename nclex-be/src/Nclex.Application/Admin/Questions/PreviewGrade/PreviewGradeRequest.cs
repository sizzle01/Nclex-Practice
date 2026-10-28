using System.Text.Json;

namespace Nclex.Application.Admin.Questions.PreviewGrade;

/// <summary>Body of POST /admin/question-versions/{id}/preview-grade.</summary>
/// <param name="Response">An answer in the item type's response shape (interfaces.md section 2); null grades a blank answer.</param>
public sealed record PreviewGradeRequest(JsonElement? Response);
