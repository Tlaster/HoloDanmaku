using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record Guard(User User, decimal Price, string Name, Guard.GuardLevel Level, DateTimeOffset Timestamp) : IMessage
{
    public enum GuardLevel
    {
        Captain = 1,
        Admiral = 2,
        Governor = 3,
    }
}

internal class GuardParser : IMessageParser
{
    public bool CanParse(string cmd)
    {
        return cmd == "GUARD_BUY";
    }

    public Task<IMessage> Parse(JObject obj)
    {
        var guardLevel = (int)obj["data"]["guard_level"];
        var userId = (int)obj["data"]["uid"];
        var userName = (string)obj["data"]["username"];
        var guardName = (string)obj["data"]["gift_name"];
        var price = (int)obj["data"]["price"];
        var number = (int)obj["data"]["num"];
        var timestamp = (long)obj["data"]["start_time"];
        var user = new User(userName, null, userId);
        var guard = new Guard(user, price, guardName, (Guard.GuardLevel)guardLevel, DateTimeOffset.FromUnixTimeSeconds(timestamp));
        return Task.FromResult<IMessage>(guard);
    }
}