using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts;

/// <summary>
/// Checks a candidate's answer against its item before it is stored (option ids exist, a Select-N
/// answer is not over N...), so grading can trust what is stored. Used by autosave and by the final
/// answers sent with Submit.
/// </summary>
public sealed class ResponseValidator(IAppDbContext db, ItemTypeRegistry itemTypes)
{
    /// <summary>
    /// The answer as it will be stored: null when it clears the answer. Problems are returned as
    /// 400 VALIDATION_FAILED. A locked item is not checked, because it refuses the write anyway.
    /// </summary>
    public async Task<JsonDocument?> ToStoredAsync(AttemptItem item, JsonElement? response, CancellationToken cancellationToken)
    {
        if (response is not { ValueKind: not JsonValueKind.Null } value)
        {
            return null;
        }

        if (!item.IsLocked)
        {
            await EnsureValidAsync(item, value, cancellationToken);
        }

        return JsonDocument.Parse(value.GetRawText());
    }

    private async Task EnsureValidAsync(AttemptItem item, JsonElement response, CancellationToken cancellationToken)
    {
        var version = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.Id == item.QuestionVersionId)
            .Select(v => new { v.ItemType, v.Content })
            .SingleAsync(cancellationToken);
        var issues = itemTypes.Get(version.ItemType).ValidateResponse(version.Content.RootElement, response);
        if (issues.Count > 0)
        {
            throw new AppException(
                CommonErrors.ValidationFailed,
                fieldErrors: [.. issues.Select(issue => new AppFieldError(issue.Path, issue.Code))]);
        }
    }
}
