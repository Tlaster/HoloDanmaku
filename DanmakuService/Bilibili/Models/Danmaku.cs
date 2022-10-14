using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record Danmaku(string Content, Badge? Badge, User User, DateTimeOffset Timestamp) : IMessage;

public record Emoji(string Name, string Url, Badge? Badge, User User, DateTimeOffset Timestamp) : IMessage;

public record Badge(string Name, int Level);

internal class DanmakuMessageParser : IMessageParser
{
    private readonly Dictionary<long, string> _faceCache = new();
    public bool CanParse(string cmd)
    {
        return cmd == "DANMU_MSG";
    }

    public async Task<IMessage> Parse(JObject obj)
    {
        var userId = (long)obj["info"][2][0];
        var userName = (string)obj["info"][2][1];
        var message = (string)obj["info"][1];
        var guardLv = (int)obj["info"][7];
        var userTitile = (string)obj["info"][5][1];
        var messageType = (int)obj["info"][0][1];
        var messageFontSize = (int)obj["info"][0][2];
        var messageColor = (int)obj["info"][0][3];
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)obj["info"][0][4]);
        var emojiToken = obj["info"][0][13];
        if (!_faceCache.TryGetValue(userId, out var face))
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://api.bilibili.com/x/space/acc/info?mid={userId}");
            var json = await response.Content.ReadAsStringAsync();
            var faceObj = JObject.Parse(json);
            if (faceObj.TryGetValue("data", out var data) && data is JObject dataObj && dataObj.TryGetValue("face", out var faceValue))
            {
                face = (string)faceValue;
                _faceCache[userId] = face;
            }
        }

        var badge = BadgeParser.ParseBadge(obj["info"][3]);
        
        var user = new User(Name: userName, Id: userId, Avatar: face);
        if (emojiToken.HasValues)
        {
            var emoji = emojiToken["url"].ToString();
            return new Emoji(Name: message, Url: emoji, User: user, Timestamp: timestamp, Badge: badge);
        }
        else
        {
            return new Danmaku(Content: message, User: user, Timestamp: timestamp, Badge: badge);
        }
    }
}

internal class BadgeParser
{
    public static Badge? ParseBadge(JToken obj)
    {
        if (!obj.HasValues)
        {
            return null;
        }

        var level = obj[0].Value<int>();
        var name = obj[1].Value<string>();
        var userName = obj[2].Value<string>();
        var roomId = obj[3].Value<int>();
        var userId = obj[12].Value<long>();
        return new Badge(Name: name, Level: level);
    }
}