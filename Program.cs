using CoffeeShopBot.Cache;
using CoffeeShopBot.Data;
using CoffeeShopBot.Models;
using CoffeeShopBot.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var connect = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connect));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();
var botToken = builder.Configuration.GetSection("BotConfiguration")
.GetValue<string>("BotToken");
if (string.IsNullOrEmpty(botToken))
{
    throw new Exception("Telegram Bot Token is not configured in appsettings.json");
}
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddHostedService<TelegramBotBackgroundService>();
builder.Services.AddTransient<MemoryCache>();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

    if (!await db.admins.AnyAsync())
    {
        var admin = new Admin
        {
            Name = "Admin"
        };

        var password = builder.Configuration["Admin:Password"];

        if (string.IsNullOrEmpty(password))
        {
            throw new Exception("Admin password is not configured");
        }

        var hasher = new PasswordHasher<Admin>();

        admin.PasswordHash = hasher.HashPassword(admin, password);

        db.admins.Add(admin);
        await db.SaveChangesAsync();
    }
}
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("index.html").RequireAuthorization();

app.MapGet("/login", () =>
{
    return Results.File("login.html", "text/html");
});
app.MapGet("/api/users", async (ApplicationContext db) =>
{
    return await db.users.ToListAsync();
}).RequireAuthorization();

app.MapPost("/api/users/change-points", async (string username, int points, ApplicationContext db, ITelegramBotClient botClient, MemoryCache cache) =>
{
    string cleanUsername = username.Replace("@", "").Trim();
    User? user = await cache.GetUserName(username);

    if (user == null)
    {
        return Results.NotFound(new { message = $"Пользователь @{cleanUsername} не найден в базе!" });
    }

    user.BonusCoint += points;

    if (user.BonusCoint < 0)
    {
        user.BonusCoint = 0;
    }

    await db.SaveChangesAsync();

   

    return Results.Ok(new
    {
        message = $"Успешно! Баланс @{user.TelegramUserName} изменен на {points}. Текущий баланс: {user.BonusCoint}"
    });
})
.RequireAuthorization();

app.MapPost("/login", async (HttpContext context, ApplicationContext db) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("login") || !form.ContainsKey("password"))
    {
        return Results.BadRequest("Неверный логин или пароль");
    }

    string? userName = form["login"];
    string? password = form["password"];

    Admin? admin = await db.admins.FirstOrDefaultAsync(u => u.Name == userName);

    if (admin == null) return Results.Redirect("/login?error=InvalidCredentials");

    var hasher = new PasswordHasher<Admin>();

    var result = hasher.VerifyHashedPassword(admin, admin.PasswordHash, password!);

    if (result != PasswordVerificationResult.Success)
    {
        return Results.Redirect("/login?error=InvalidCredentials");
    }

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName!) };
    var identity = new ClaimsIdentity(claims, "Cookies");
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(principal);
    return Results.Redirect("/");


});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});
app.MapGet("/api/users-deleted/{telegramId}", async (long telegramId, ApplicationContext db) =>
{
    var user = await db.users.FindAsync(telegramId);
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });
    db.users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Пользователь удален" });
});
app.Run();
