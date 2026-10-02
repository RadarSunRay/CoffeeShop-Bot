using CoffeeShopBot.State;
using Telegram.Bot.Types;

namespace CoffeeShopBot.Services
{
    public interface IUserService
    {
        public Task<Models.User> CreateUserAsync(SceneContext scene, string userName);
        public Task SetPhoneNumberAsync(SceneContext scene, string phoneNumber);
        public Task ParseRefererId(SceneContext scene, string text);
    }
}
