using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record SuperChat(string Content, Badge? Badge, User User, DateTimeOffset Timestamp, decimal Price, int TimeLength) : Danmaku(Content, Badge, User, Timestamp);

internal class SuperChatParser : IMessageParser
{
    public bool CanParse(string cmd)
    {
        return cmd == "SUPER_CHAT_MESSAGE";
    }

    public async Task<IMessage> Parse(JObject obj)
    {
        var message = (string)obj["data"]["message"];
        var messageTrans = (string)obj["data"]["message_trans"];
        var userId = (long)obj["data"]["uid"];
        var userName = (string)obj["data"]["user_info"]["uname"];
        var face = (string)obj["data"]["user_info"]["face"];
        var ts = (long)obj["data"]["ts"];
        var scid = (int)obj["data"]["id"];
        var price = (decimal)obj["data"]["price"];
        var rate = (int)obj["data"]["rate"];
        var timeLength = (int)obj["data"]["time"];
        var timestamp = (long)obj["data"]["start_time"];
        var user = new User(Name: userName, Id: userId, Avatar: face);
        return new SuperChat(message, null, user, DateTimeOffset.FromUnixTimeSeconds(timestamp), price, timeLength);
    }
}