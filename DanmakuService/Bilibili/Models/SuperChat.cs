using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record SuperChat(string Content, Badge? Badge, User User, DateTimeOffset Timestamp, DateTimeOffset EndTimestamp, decimal Price, decimal Rate, int TimeLength) : Danmaku(Content, Badge, User, Timestamp);

internal class SuperChatParser : IMessageParser
{
    public bool CanParse(string cmd)
    {
        return cmd == "SUPER_CHAT_MESSAGE";
    }

    public Task<IMessage> Parse(JObject obj)
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
        var endTimestamp = (long)obj["data"]["end_time"];
        var badgeName = (string)obj["data"]["medal_info"]["medal_name"];
        var badgeLevel = (int)obj["data"]["medal_info"]["medal_level"];
        var badgeRoomId = (int)obj["data"]["medal_info"]["anchor_roomid"];
        var badgeAnchorName = (string)obj["data"]["medal_info"]["anchor_uname"];
        var badgeUserId = (long)obj["data"]["medal_info"]["target_id"];
        var badge = new Badge(badgeName, badgeLevel);
        var user = new User(Name: userName, Id: userId, Avatar: face);
        return Task.FromResult<IMessage>(new SuperChat(message, badge, user,
            DateTimeOffset.FromUnixTimeSeconds(timestamp), DateTimeOffset.FromUnixTimeSeconds(endTimestamp), price,
            rate, timeLength));
    }
}