using System.Text.Json;
using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.Adaptive;

/// <summary>
/// Body of POST /attempts/{id}/items/{position}/answer (api.md section 4): the confirmed answer to
/// the current question of an adaptive exam.
/// </summary>
/// <param name="Response">The answer; the NCLEX does not let a question be skipped, so it is required.</param>
/// <param name="ClientSeq">Orders the answer against autosaves still in flight.</param>
public sealed record AnswerAdaptiveItemRequest(JsonElement? Response, int? ClientSeq);

public sealed class AnswerAdaptiveItemRequestValidator : AbstractValidator<AnswerAdaptiveItemRequest>
{
    public AnswerAdaptiveItemRequestValidator()
    {
        RuleFor(r => r.Response)
            .Must(r => r is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined })
            .WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.ClientSeq).NotNull().WithErrorCode(ValidationErrorCodes.Required);
        // As in autosave: stored values start at 0, so the first real write is 1.
        RuleFor(r => r.ClientSeq).GreaterThan(0).When(r => r.ClientSeq is not null).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

/// <summary>Body returned by POST /answer.</summary>
/// <param name="Ended">True when the exam stopped; the attempt is then submitted and its result is ready.</param>
/// <param name="Attempt">The attempt after the answer: on the next question, or submitted.</param>
public sealed record AnswerAdaptiveItemResult(bool Ended, AttemptState Attempt);
