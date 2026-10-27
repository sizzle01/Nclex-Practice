using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.VerifyEmail;

/// <summary>
/// POST /auth/verify-email: uses a verification link's token and marks the account's email as
/// verified. No sign-in is needed, so the link works on any device.
/// </summary>
public sealed class VerifyEmailHandler(IAppDbContext db, EmailTokenService emailTokens, TimeProvider clock)
{
    public async Task HandleAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var emailToken = await emailTokens.FindUsableAsync(request.Token, EmailTokenPurpose.VerifyEmail, now, cancellationToken)
            ?? throw new AppException(AuthErrors.InvalidToken);

        emailToken.Consume(now);
        emailToken.User!.MarkEmailVerified(now); // FindUsableAsync loads the user

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The same link was used by another request between our read and our write.
            throw new AppException(AuthErrors.InvalidToken);
        }
    }
}
