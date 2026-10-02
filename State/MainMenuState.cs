using CoffeeShopBot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class MainMenuState : IBotState
    {
        private InlineKeyboardMarkup GetMainMenuKeyboard()
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("💳 Мой баланс", "get_balance")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("☕️ О кофейне", "get_info")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📞 Контакты", "get_contact")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🤝 Пригласить друга", "get_invite")
                }
            });
            return keyboard;
        }

        public async Task EnterHandleAsync(SceneContext scene)
        {
            await scene.botClient.SendMessage(
                chatId: scene.chatId,
                text: "Главное меню:",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: scene.cancellationtoken);
        }

        public async Task HandleInputAsync(SceneContext scene, Update update)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callBack = update.CallbackQuery;
                string? data = callBack.Data;
                await scene.botClient.AnswerCallbackQuery(callBack.Id, cancellationToken: scene.cancellationtoken);
                switch (data)
                {
                    case "get_balance":
                        {
                            await scene.ChangeState(scene.UserBalanceState);
                            break;
                        }
                    case "get_info":
                        {
                            await scene.ChangeState(scene.InfoCoffeeState);
                            break;
                        }
                    case "get_contact":
                        {
                            await scene.ChangeState(scene.ContactState);
                            break;
                        }
                    case "get_invite":
                        {
                            await scene.ChangeState(scene.ReferalSystemState);
                            break;
                        }
                }
            }
        }
    }
}
