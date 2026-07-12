using CoffeeShopBot.Data;
using CoffeeShopBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoffeeShopBot.Cache;
public class MemoryCache
{
    private ApplicationContext db;
    private IMemoryCache memoryCache;
    public MemoryCache(ApplicationContext _db, IMemoryCache _memory)
    {
        db = _db;
        memoryCache = _memory;
    }

    public async Task<User> GetUserName(string userName)
    {
        if (memoryCache.TryGetValue(userName, out User? usercache))
        {
            return usercache;
        }
        User? user = await db.users.FirstOrDefaultAsync(u => u.TelegramUserName.ToLower() == userName.ToLower());
        if (user == null)
        {
            if (user != null)
            {
                memoryCache.Set(user.TelegramUserName, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
        }
        return user;
    }
    public async Task<User> GetUserId(long id)
    {
        if (memoryCache.TryGetValue(id, out User? usercache))
        {
            return usercache;
        }
        User? user = await db.users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return null;
        }
        
        memoryCache.Set(user.TelegramUserName, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            
        return user;
    }
}