using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Reveal;
using Nclex.Application.Common;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions.PreviewGrade;

/// <summary>
/// POST /admin/question-versions/{id}/preview-grade: "test the key" (api.md section 8). Grades a
/// sample answer with the real grader against any version, and stores nothing, so an author or
/// reviewer can check partial credit before candidates meet the item.
/// </summary>
public sealed class PreviewGradeHandler(IAppDbContext db, ItemTypeRegistry itemTypes)
{
    public async Task<ItemGrade> HandleAsync(Guid versionId, PreviewGradeRequest request, CancellationToken cancellationToken)
    {
        var version = await db.QuestionVersions.AsNoTracking().SingleOrDefaultAsync(v => v.Id == versionId, cancellationToken)
            ?? throw new AppException(AdminErrors.VersionNotFound);
        var module = itemTypes.Get(version.ItemType);
        var content = version.Content.RootElement;

        JsonElement? response = request.Response is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } given ? given : null;
        if (response is { } answer)
        {
            // The same checks autosave runs, so a preview never grades an answer a candidate couldn't send.
            var issues = module.ValidateResponse(content, answer);
            if (issues.Count > 0)
            {
                throw AuthoringValidator.Invalid(issues.Select(i => i with { Path = i.Path.StartsWith("response", StringComparison.Ordinal) ? i.Path : $"response.{i.Path}" }));
            }
        }

        var grade = module.Grade(content, version.AnswerKey.RootElement, response);
        return new ItemGrade(grade.Points, grade.MaxPoints, grade.IsCorrect, grade.Detail);
    }
}
