using Telegram.Bot;
using Telegram.Bot.Types;

namespace CoffeeShopBot.State
{
    public class SceneContext
    {
        public ITelegramBotClient botClient { get; private set; }
        public IServiceProvider serviceProvider { get; private set; }
        public long chatId { get; private set; }
        public CancellationToken cancellationtoken { get; private set; }
        public IBotState CurrentState { get; set; }
        public MainMenuState MainMenuState { get; private set; }
        public UserBalanceState UserBalanceState { get; private set; }
        public InfoCoffeeState InfoCoffeeState { get; private set; }
        public ContactInfoState ContactState { get; private set; }
        public ReferalSystemState ReferalSystemState { get; private set; }
        public SetPhoneNumberState SetPhoneNumberState { get; private set; }
        public SceneContext()
        {
            MainMenuState = new MainMenuState();
            UserBalanceState = new UserBalanceState();
            InfoCoffeeState = new InfoCoffeeState();
            ContactState = new ContactInfoState();
            ReferalSystemState = new ReferalSystemState();
            SetPhoneNumberState = new SetPhoneNumberState();
        }

        public async Task ChangeState(IBotState state)
        {
            CurrentState = state;
            await state.EnterHandleAsync(this);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, IServiceProvider service, long chatId, CancellationToken cancellation)
        {
            this.botClient = botClient;
            serviceProvider = service;
            this.chatId = chatId;
            cancellationtoken = cancellation;
        }

        public async Task ProccessMessageAsync(Update update)
        {
            await CurrentState.HandleInputAsync(this, update);
        }
    }
}
