using System.Data.Common;
using System.Diagnostics;
using CoffeeShopBot.Data;
using CoffeeShopBot.State;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoffeeShopBot.Services;
public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botclient;
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserContext _user;
    public TelegramBotBackgroundService(ITelegramBotClient botClient,
    ILogger<TelegramBotBackgroundService> logger,
    IServiceProvider serviceProvider,
    UserContext user)
    {
        _botclient = botClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _user = user;
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
        long? chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;

        if (chatId == null) return;

        var context = _user.GetContext(chatId.Value);
        if (context.CurrentState == null)
        {
            context.CurrentState = context.MainMenuState;
        }
        await context.HandleUpdateAsync(botClient, _serviceProvider, chatId.Value, cancellationToken);

        if (update.Message is {Text: { } text} && text.StartsWith("/start"))
        {
            using (var scope = context.serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var user = await userService.CreateUserAsync(context, update.Message.From?.Username ?? "No name");

                if (user.isNewUser)
                    await userService.ParseRefererId(context, text);

                if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        await context.ChangeState(context.SetPhoneNumberState);
                    }
                    else
                        await context.ChangeState(context.MainMenuState);
            }
            return;
        }

        await context.ProccessMessageAsync(update);

        _logger.LogInformation($"Получено сообщение: {update.Message.Text}, в чате: {chatId}. {DateTime.Now}", update.Message.Text, chatId);
        
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при получении обновления Telegram.Bot");
        return Task.CompletedTask;
    }
}