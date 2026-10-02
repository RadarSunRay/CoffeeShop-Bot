using CoffeeShopBot.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeShopBot.EndPoints
{
    public static class LogEndpoints
    {
        public static void MapLogin(this IEndpointRouteBuilder app)
        {
            app.MapGet("/login", () =>
            {
                return Results.File("login.html", "text/html");
            });

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
        }

        public static void MapLogout(this IEndpointRouteBuilder app)
        {
            app.MapGet("/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/login");
            });
        }
    }
}
