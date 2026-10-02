using CoffeeShopBot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using CoffeeShopBot.Cache;
using CoffeeShopBot.Models;

namespace CoffeeShopBot.EndPoints
{
    public static class UserEndPoints
    {
        public static void MapGetUser(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users", async (ApplicationContext db) =>
            {
                return await db.users.ToListAsync();
            }).RequireAuthorization();
        }
        public static void MapUserBonusCoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/change-points", async (string username, int points, ApplicationContext db, ITelegramBotClient botClient, MemoryCache cache) =>
            {
                string cleanUsername = username.Replace("@", "").Trim();
                User? user = await cache.GetUserName(username);

                if (user == null)
                {
                    return Results.NotFound(new { message = $"Пользователь @{cleanUsername} не найден в базе!" });
                }

                user.BonusCoint += points;

                if (user.BonusCoint < 0)
                {
                    user.BonusCoint = 0;
                }

                await db.SaveChangesAsync();

                await botClient.SendMessage(
                    chatId: user.Id,
                    text: $"Вам начислено {points} бонусов🎉🎉🎉\nТекущий баланс: {user.BonusCoint}",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
                return Results.Ok(new
                {
                    message = $"Успешно! Баланс @{user.TelegramUserName} изменен на {points}. Текущий баланс: {user.BonusCoint}"
                });
            }).RequireAuthorization();
        }

        public static void MapDeleteUser(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/users-deleted/{telegramId}", async (long telegramId, ApplicationContext db) =>
            {
                var user = await db.users.FindAsync(telegramId);
                if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });
                db.users.Remove(user);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Пользователь удален" });
            });
        }
    }
}
