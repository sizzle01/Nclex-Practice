using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Application.Users;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.Login;

/// <summary>POST /auth/login: checks the email and password and starts a new session.</summary>
public sealed class LoginHandler(IAppDbContext db, IPasswordHasher passwordHasher, SessionService sessions)
{
    public async Task<SignInResult> HandleAsync(LoginRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        var account = user is { IsDeleted: false } ? user : null;

        // Always verify, even when there is no account, so unknown emails take as long as wrong passwords.
        if (!passwordHasher.Verify(request.Password, account?.PasswordHash) || account is null)
        {
            throw new AppException(AuthErrors.InvalidCredentials);
        }

        var session = sessions.Start(account, client);
        await db.SaveChangesAsync(cancellationToken);
        return new SignInResult(UserProfile.From(account), session);
    }
}
