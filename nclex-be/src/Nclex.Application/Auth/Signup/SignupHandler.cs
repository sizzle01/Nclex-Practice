using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Application.Users;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.Signup;

/// <summary>
/// POST /auth/signup: creates a free, unverified candidate account, queues the verification
/// email and signs the user in. The account, its session, the verification token and the outbox
/// row are saved in one transaction (ADR-7), so there is never an account without its email.
/// </summary>
public sealed class SignupHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    SessionService sessions,
    AccountEmails accountEmails,
    TimeProvider clock)
{
    public async Task<SignInResult> HandleAsync(SignupRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        // The email column is citext, so this comparison ignores case.
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new AppException(AuthErrors.EmailTaken);
        }

        var user = User.Register(
            request.Email,
            passwordHasher.Hash(request.Password),
            request.Track!.Value, // the validator guarantees a value
            AccountRules.NormalizeDisplayName(request.DisplayName),
            clock.GetUtcNow());
        db.Users.Add(user);
        var session = sessions.Start(user, client);
        await accountEmails.QueueVerificationAsync(user, cancellationToken);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // Another sign-up with the same email won the race between the check above and this save.
            throw new AppException(AuthErrors.EmailTaken);
        }

        return new SignInResult(UserProfile.From(user), session);
    }
}
