using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Performance.RebuildSnapshots;

public sealed class RebuildSnapshotsRequestValidator : AbstractValidator<RebuildSnapshotsRequest>
{
    public RebuildSnapshotsRequestValidator()
    {
        RuleFor(r => r.UserId).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
