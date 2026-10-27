using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;

namespace Nclex.Application.Auth.ForgotPassword;

/// <summary>
/// POST /auth/password/forgot: emails a 30-minute reset link if an account has this email.
/// The API answers 202 either way, so the response never reveals whether an account exists
/// (architecture.md section 6, "Brute force").
/// </summary>
public sealed class ForgotPasswordHandler(IAppDbContext db, AccountEmails accountEmails)
{
    public async Task HandleAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        // The email column is citext, so this comparison ignores case.
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is not { IsDeleted: false })
        {
            return;
        }

        await accountEmails.QueuePasswordResetAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
