using System.Data.Common;
using CoffeeShopBot.Data;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.Service;
public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botclient;
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    public TelegramBotBackgroundService(ITelegramBotClient botClient,
    ILogger<TelegramBotBackgroundService> logger,
    IServiceProvider serviceProvider)
    {
        _botclient = botClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram Bot Hosted Service запущен");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botclient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Telegram Bot начал слушать сообщения");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;
        var chatId = message.Chat.Id;
        if (message.Contact is { } contact)
        {
            string phoneNumber = contact.PhoneNumber;

            if(!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }
            _logger.LogInformation($"Получен контакт от чата {chatId} - {phoneNumber}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                var user = await db.users.FindAsync(chatId);

                if (user != null)
                {
                    user.PhoneNumber = phoneNumber;
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"Пользователю {user.TelegramUserName} привязан номер {phoneNumber}");
                }
            }
            await botClient.SendMessage(
                chatId: chatId,
                text: $"✅ <b>Регистрация успешно завершена!</b>\n\nНомер {phoneNumber} успешно привязан к вашей бонусной карте.\n\nТеперь вы можете копить баллы. Нажмите на кнопки меню ниже!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken
            );
            return;
        }
        if (message.Text is not { } messageText) return;

        _logger.LogInformation($"Получено сообщение: {messageText}, в чате: {chatId}.", messageText, chatId);
        
        if (messageText == "/start")
        {
            string userName = message.From?.Username ?? "No name";

            bool isNewUser = false;

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                var user = await db.users.FindAsync(chatId);

                if (user == null)
                {
                    user = new Models.User
                    {
                        Id = chatId,
                        TelegramUserName = userName,
                        PhoneNumber = "",
                        BonusCoint = 0
                    };
                    db.users.Add(user);
                    await db.SaveChangesAsync();
                    isNewUser = true;
                    _logger.LogInformation($"Новый пользователь {userName} записан в БД");
                }
                else if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    isNewUser = true;
                }
            }
            if (isNewUser)
            {
            await botClient.SendMessage
            (
                chatId: chatId,
                text: $"👋 Привет, {message.From?.FirstName}!\n\nДля участия в бонусной системе нашей кофейни, пожалуйста, подтвердите свой номер телефона, нажав на кнопку ниже 👇",
                replyMarkup: GetContactMenuKeyboar(),
                cancellationToken: cancellationToken
            );
            }
            else
            {
                await botClient.SendMessage
            (
                chatId: chatId,
                text: $"С возвращением {message.From?.FirstName}!",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken
            );
            }
        }
        else if (messageText == "💳 Мой баланс")
        {
            int currentBonus = 0;
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                var user = await db.users.FindAsync(chatId);

                if (user != null)
                {
                    currentBonus = user.BonusCoint;
                }
            }
            await botClient.SendMessage
            (
                chatId: chatId,
                text: $"💳 Ваш текущий баланс: {currentBonus} бонусов.\n\nКопите бонусы и оплачивайте ими скидку в нашей кофейне!",
                cancellationToken: cancellationToken
            );
        }
        else if (messageText == "☕️ О кофейне")
        {
            await botClient.SendMessage
            (
                chatId: chatId,
                text: "☕️ <b>Наша Кофейня</b>\n\nМы варим лучший кофе в городе из свежеобжаренной 100% арабики! У нас всегда свежая выпечка и уютная атмосфера.\n\n📍 Адрес: ул. Программистов, д. 8. Ждем вас",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken
            );
        }
        else if (messageText == "📞 Контакты")
        {
            
            await botClient.SendMessage
            (
                chatId: chatId,
                text: "📞 <b>Наши контакты</b>\n\n" +
              "📱 Телефон: +7 (999) 123-45-67\n" +
              "🌐 Сайт: coffee-net.ru\n" +
              "💬 По вопросам франшизы: @coffee_boss\n\n" +
              "Работаем ежедневно с 08:00 до 22:00.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await botClient.SendMessage
            (
                chatId: chatId,
                text: "Чтобы использовать бота напишите /start или используйте кнопки ниже",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken
            );
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при получении обновления Telegram.Bot");
        return Task.CompletedTask;
    }

    private ReplyKeyboardMarkup GetMainMenuKeyboard()
    {
        var keyboard = new ReplyKeyboardMarkup(new []
        {
            new KeyboardButton[] {"💳 Мой баланс"},
            new KeyboardButton[] {"☕️ О кофейне", "📞 Контакты"}
        })
        {
            ResizeKeyboard = true
        };
        return keyboard;
    }
    
    private ReplyKeyboardMarkup GetContactMenuKeyboar()
    {
        var keyboard = new ReplyKeyboardMarkup(new []
        {
            new KeyboardButton("📱 Поделиться номером телефона") {RequestContact = true}
        })
        {
            ResizeKeyboard = true
        };
        return keyboard;
    }

}