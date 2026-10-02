using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class ReferalSystemState : IBotState
    {
        private InlineKeyboardMarkup GetMenuBalanceKeyboard()
        {
            var keyboard = InlineKeyboardButton.WithCallbackData("⬅ Назад ⬅", "get_back");
            return keyboard;
        }

        public async Task EnterHandleAsync(SceneContext sceneContext)
        {
            string botName = "artemswet_bot";
            string refLink = $"https://t.me/{botName}?start={sceneContext.chatId}";

            await sceneContext.botClient.SendMessage(
                chatId: sceneContext.chatId,
                text: $"🎁 Дарим 50 бонусов за каждого друга!\n\n" +
              $"Отправь эту ссылку другу, и когда он запустит бота, ты получишь баллы:\n\n" +
              $"`{refLink}`",
                replyMarkup: GetMenuBalanceKeyboard(),
              parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
              cancellationToken: sceneContext.cancellationtoken
            );
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
