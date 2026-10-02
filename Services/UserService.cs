using CoffeeShopBot.Data;
using CoffeeShopBot.State;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace CoffeeShopBot.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationContext _db;
        public UserService(ApplicationContext db)
        {
            _db = db;
        }
        public async Task<Models.User> CreateUserAsync(SceneContext scene, string? userName)
        {
            var user = await _db.users.FirstOrDefaultAsync(x => x.Id == scene.chatId);

            if (user == null)
            {
                user = new Models.User
                {
                    Id = scene.chatId,
                    TelegramUserName = userName ?? "No name",
                    PhoneNumber = "",
                    BonusCoint = 0,
                    isNewUser = true
                };
                _db.users.Add(user);
                await _db.SaveChangesAsync();
            }
            return user;
        }

        public async Task SetPhoneNumberAsync(SceneContext scene, string phoneNumber)
        {
            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }

            var user = await _db.users.FirstOrDefaultAsync(x => x.Id == scene.chatId);

            if (user != null)
            {
                user.PhoneNumber = phoneNumber;
                await _db.SaveChangesAsync();
            }

        }

        public async Task ParseRefererId(SceneContext scene, string text)
        {
            var parts = text.Split(' ');
            var currentUser = await _db.users.FirstOrDefaultAsync(x => x.Id == scene.chatId);
            if (currentUser != null)
            {
                currentUser.isNewUser = false;
            }
            if (parts.Length > 1)
            {
                string referrerIdStr = parts[1];

                if (long.TryParse(referrerIdStr, out long referrId))
                {
                    if (referrId != scene.chatId)
                    {
                        var referrer = await _db.users.FirstOrDefaultAsync(x => x.Id == referrId);

                        if (referrer != null)
                        {
                            referrer.BonusCoint += 50;

                            await scene.botClient.SendMessage(
                                chatId: referrId,
                                text: "🎉 Ура! По вашей ссылке зарегистрировался новый друг. Вам начислено <b>50 бонусов</b>!",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                            );
                        }

                    }
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
