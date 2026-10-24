using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Users.UpdateMe;

/// <summary>PATCH /me: edits the signed-in user's profile and returns the updated profile.</summary>
public sealed class UpdateMeHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<UserProfile> HandleAsync(UpdateMeRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Id == currentUser.UserId && u.DeletedAt == null, cancellationToken)
            ?? throw new AppException(CommonErrors.Unauthenticated);

        user.UpdateProfile(
            displayName: request.DisplayName?.Trim(),
            request.Track,
            request.Timezone,
            request.TargetExamDate,
            clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return UserProfile.From(user);
    }
}
