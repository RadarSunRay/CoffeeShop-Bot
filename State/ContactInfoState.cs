using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class ContactInfoState : IBotState
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
                text: "📞 <b>Наши контакты</b>\n\n" +
              "📱 Телефон: +7 (999) 123-45-67\n" +
              "🌐 Сайт: coffee-net.ru\n" +
              "💬 По вопросам франшизы: @coffee_boss\n\n" +
              "Работаем ежедневно с 08:00 до 22:00.",
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
