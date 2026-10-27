using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Users;

/// <summary>Loads the signed-in user's account, tracked so the use case can change it.</summary>
public sealed class CurrentAccount(IAppDbContext db, ICurrentUser currentUser)
{
    /// <summary>
    /// The signed-in user. A valid access token for a deleted account is treated as signed out
    /// (401), which is how a deleted account's remaining tokens stop working.
    /// </summary>
    public async Task<User> LoadAsync(CancellationToken cancellationToken) =>
        await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.UserId && u.DeletedAt == null, cancellationToken)
        ?? throw new AppException(CommonErrors.Unauthenticated);
}
