using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class InfoCoffeeState : IBotState
    {
        private InlineKeyboardMarkup GetInfoKeyboard()
        {
            var keyboard = InlineKeyboardButton.WithCallbackData("⬅ Назад ⬅", "get_back");
            return keyboard;
        }

        public async Task EnterHandleAsync(SceneContext sceneContext)
        {
            await sceneContext.botClient.SendMessage
            (
                chatId: sceneContext.chatId,
                text: "☕️ <b>Наша Кофейня</b>\n\nМы варим лучший кофе в городе из свежеобжаренной 100% арабики! У нас всегда свежая выпечка и уютная атмосфера.\n\n📍 Адрес: ул. Нечаева, д. 8. Ждем вас",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: GetInfoKeyboard(),
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
