namespace CoffeeShopBot.Models;
public class User
{
    public long Id {get;set;}
    public string TelegramUserName {get;set;} = string.Empty;
    public string PhoneNumber {get;set;} = string.Empty;
    public int BonusCoint {get;set;} = 0;
}