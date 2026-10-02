using CoffeeShopBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.State
{
    public class SetPhoneNumberState : IBotState
    {
        private ReplyKeyboardMarkup GetPhoneKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            KeyboardButton.WithRequestContact("📱 Поделиться номером телефона")
        })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        public async Task EnterHandleAsync(SceneContext sceneContext)
        {
            await sceneContext.botClient.SendMessage(
                chatId: sceneContext.chatId,
                text: "Чтобы получать бонусы, зарегистрируйте номер телефона",
                replyMarkup: GetPhoneKeyboard(),
                cancellationToken: sceneContext.cancellationtoken);
        }

        public async Task HandleInputAsync(SceneContext scene, Update update)
        {
            if (update.Message?.Contact is { } contact)
            {
                using (var scope = scene.serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    await userService.SetPhoneNumberAsync(scene, contact.PhoneNumber);

                    await scene.botClient.SendMessage(
                        chatId: scene.chatId,
                        text: "✅ Номер телефона успешно привязан!",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: scene.cancellationtoken
                    );
                    await scene.ChangeState(scene.MainMenuState);
                }
            }
            else
            {
                await scene.botClient.SendMessage(
                    chatId: scene.chatId,
                    text: "Пожалуйста, нажмите на кнопку «📱 Поделиться номером телефона» внизу экрана.",
                    replyMarkup: GetPhoneKeyboard(),
                    cancellationToken: scene.cancellationtoken
                );
            }
        }
    }
}

