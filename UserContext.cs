using CoffeeShopBot.State;
using System.Collections.Concurrent;

namespace CoffeeShopBot
{
    public class UserContext
    {
        private readonly ConcurrentDictionary<long, SceneContext> _concurrent = new();

        public SceneContext GetContext(long userId)
        {
            return _concurrent.GetOrAdd(userId, id => new SceneContext());
        }
    }
}
