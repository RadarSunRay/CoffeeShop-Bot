using CoffeeShopBot.State;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace CoffeeShopBot.Models;
public class User
{
    public long Id {get;set;}
    public string TelegramUserName {get;set;} = string.Empty;
    public string PhoneNumber {get;set;} = string.Empty;
    public int BonusCoint {get;set;} = 0;
    public bool isNewUser { get; set; } = true;
}