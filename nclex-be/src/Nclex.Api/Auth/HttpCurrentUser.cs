using Nclex.Application.Abstractions;
using Nclex.Infrastructure.Security;

namespace Nclex.Api.Auth;

/// <summary>Reads the signed-in user from the validated access token on the current request.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var subject = httpContextAccessor.HttpContext?.User.FindFirst(AccessTokenClaims.Subject)?.Value;
            return Guid.TryParse(subject, out var userId)
                ? userId
                : throw new InvalidOperationException("No signed-in user. Is the endpoint missing [Authorize]?");
        }
    }

    public Guid? SessionFamilyId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(AccessTokenClaims.SessionId)?.Value, out var familyId)
            ? familyId
            : null;
}
