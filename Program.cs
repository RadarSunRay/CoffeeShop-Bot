using CoffeeShopBot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using CoffeeShopBot.Models;
using CoffeeShopBot.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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

var app = builder.Build();


app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("index.html").RequireAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    // html-форма для ввода логина/пароля
    string loginForm = @"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>METANIT.COM</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <form method='post'>
            <p>
                <label>Login</label><br />
                <input name='email' />
            </p>
            <p>
                <label>Password</label><br />
                <input type='password' name='password' />
            </p>
            <input type='submit' value='Login' />
        </form>
    </body>
    </html>";
await context.Response.WriteAsync(loginForm);
});

app.MapGet("/api/users", async (ApplicationContext db) =>
{
    return await db.users.ToListAsync();
}).RequireAuthorization();

app.MapPost("/api/users/change-points", async (string phoneNumber, ApplicationContext db, int points, ITelegramBotClient botClient) =>
{
    if (!phoneNumber.StartsWith("+"))
    {
        phoneNumber = "+" + phoneNumber;
    }

    User? user = await db.users.FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber);

    if (user == null) return Results.NotFound(new {message = $"Пользователь с таким номером {phoneNumber} не найден"});
    
    user.BonusCoint += points;
    if (user.BonusCoint < 0)
    {
        user.BonusCoint = 0;
    }
    await db.SaveChangesAsync();
    await botClient.SendMessage(
        chatId: user.Id,
        text: $"🎉 Ваш баланс обновлен!\nВам {(points > 0 ? "начислено" : "списано")} {Math.Abs(points)} бонусов.\nТекущий баланс: {user.BonusCoint} бонусов.",
        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
    );
    System.Console.WriteLine($"Уведомление отправлено пользователю {user.TelegramUserName} ({user.Id})");
    
    return Results.Ok(new {message = $"Успешно! Баланс пользователя {user.TelegramUserName} изменен на {points}. Текущий баланс {user.BonusCoint}"});
}).RequireAuthorization();

app.MapPost("/login", async(HttpContext context, ApplicationContext db) =>
{
    var form = context.Request.Form;

    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
    {
        return Results.BadRequest("Неверный логин или пароль");
    }

    string? userName = form["email"];
    string? password = form["password"];

    Admin? admin = await db.admins.FirstOrDefaultAsync(u => u.userName == userName && u.password == password);

    if (admin != null)
    {
        var claims = new List<Claim> {new Claim(ClaimTypes.Name, userName)};
        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);
        await context.SignInAsync(principal);
        return Results.Ok(new {message = "Успешный вход!"});
    }
    else
    {
        return Results.BadRequest(new {message = "Неверный логин или пароль"});
    }

});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});
app.Run();
