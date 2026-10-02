using CoffeeShopBot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class UserBalanceState : IBotState
    {
        private InlineKeyboardMarkup GetMenuBalanceKeyboard()
        {
            var keyboard = InlineKeyboardButton.WithCallbackData("⬅ Назад ⬅", "get_back");
            return keyboard;
        }

        public async Task EnterHandleAsync(SceneContext sceneContext)
        {
            using (var scope = sceneContext.serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                var user = await db.users.FirstOrDefaultAsync(x => x.Id == sceneContext.chatId);

                if (user != null)
                {
                    await sceneContext.botClient.SendMessage
                    (
                    chatId: user.Id,
                    text: $"💳 Ваш текущий баланс: {user.BonusCoint} бонусов.\n\nКопите бонусы и оплачивайте ими скидку в нашей кофейне!",
                    replyMarkup: GetMenuBalanceKeyboard(),
                    cancellationToken: sceneContext.cancellationtoken
                    );
                }
            }

        }

        public async Task HandleInputAsync(SceneContext sceneContext, Update update)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                switch (update.CallbackQuery.Data)
                {
                    case "get_back":
                        {
                            await sceneContext.botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: sceneContext.cancellationtoken);
                            await sceneContext.ChangeState(sceneContext.MainMenuState);
                            break;
                        }
                }
            }
        }
    }
}
