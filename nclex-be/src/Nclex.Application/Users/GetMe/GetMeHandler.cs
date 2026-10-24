using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Users.GetMe;

/// <summary>GET /me: the signed-in user's profile.</summary>
public sealed class GetMeHandler(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<UserProfile> HandleAsync(CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == currentUser.UserId && u.DeletedAt == null, cancellationToken);

        // A valid token for a missing or deleted account: treat the caller as signed out.
        return user is null
            ? throw new AppException(CommonErrors.Unauthenticated)
            : UserProfile.From(user);
    }
}
