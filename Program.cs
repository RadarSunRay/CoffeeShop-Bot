using CoffeeShopBot;
using CoffeeShopBot.Cache;
using CoffeeShopBot.Data;
using CoffeeShopBot.EndPoints;
using CoffeeShopBot.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
// DB
var connect = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connect));
// Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
// BotToken
var botToken = builder.Configuration.GetSection("BotConfiguration")
.GetValue<string>("BotToken");
if (string.IsNullOrEmpty(botToken))
{
    throw new Exception("Telegram Bot Token is not configured in appsettings.json");
}
// Services
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddHostedService<TelegramBotBackgroundService>();
builder.Services.AddTransient<MemoryCache>();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<UserContext>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();

    await seeder.SeedAsync();
}
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("index.html").RequireAuthorization();

app.MapLogin();

app.MapGetUser();

app.MapUserBonusCoint();

app.MapDeleteUser();

app.MapLogout();

app.Run();
