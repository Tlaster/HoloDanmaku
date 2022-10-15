using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record Gift(string Name, User User, Badge Badge, decimal Price, int Amount, bool IsGold, DateTimeOffset Timestamp) : IMessage;


internal class GiftParser : IMessageParser
{
    public bool CanParse(string cmd)
    {
        return cmd == "SEND_GIFT";
    }

    public Task<IMessage> Parse(JObject obj)
    {
        var userId = (long)obj["data"]["uid"];
        var userName = (string)obj["data"]["uname"];
        var totalCoin = (int)obj["data"]["total_coin"];
        var amount = (int)obj["data"]["num"];
        var giftName = (string)obj["data"]["giftName"];
        var giftId = (int)obj["data"]["giftId"];
        var giftType = (int)obj["data"]["giftType"];
        var giftPrice = (float)obj["data"]["price"];
        var guardLevel = (int)obj["data"]["guard_level"];
        var giftAction = (string)obj["data"]["action"];
        var coinType = (string)obj["data"]["coin_type"];
        var isGoldGift = coinType == "gold";
        var timestamp = (long)obj["data"]["timestamp"];
        var avatarUrl = (string)obj["data"]["face"];

        var badgeName = (string)obj["data"]["medal_info"]["medal_name"];
        var badgeLevel = (int)obj["data"]["medal_info"]["medal_level"];
        var badgeRoomId = (int)obj["data"]["medal_info"]["anchor_roomid"];
        var badgeAnchorName = (string)obj["data"]["medal_info"]["anchor_uname"];
        var badgeUserId = (long)obj["data"]["medal_info"]["target_id"];
        var badge = new Badge(badgeName, badgeLevel);
        
        var user = new User(userName, avatarUrl, userId);
        var gift = new Gift(giftName, user, badge, (decimal)giftPrice, amount, isGoldGift, DateTimeOffset.FromUnixTimeSeconds(timestamp));
        return Task.FromResult<IMessage>(gift);
    }
}