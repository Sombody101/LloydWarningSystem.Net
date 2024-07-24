using DSharpPlus.Entities;
using LloydWarningSystem.Net.Models;

namespace LloydWarningSystem.Net.Context;

internal static class DbHelpers
{
    public static async ValueTask<UserDbEntity> FindOrCreateUserAsync(this LloydContext db, DiscordUser duser)
    {
        var user = await db.Users.FindAsync(duser.Id);

        if (user is not null)
            return user;

        user = new UserDbEntity()
        {
            Username = duser.Username,
            Id = duser.Id,
        };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        return user;
    }
}
