using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public record Interact(User User, Badge Badge, Interact.MessageType Type, ImmutableList<Interact.Identity> Identities, DateTimeOffset Timestamp) : IMessage
{
    public enum MessageType
    {
        Unknown = 0,
        Entry = 1,
        Attention = 2,
        Share = 3,
        SpecialAttention = 4,
        MutualAttention = 5,
    }

    public enum Identity
    {
        Unknown = 0,
        Normal = 1,
        Manager = 2,
        Fans = 3,
        Vip = 4,
        SVip = 5,
        GuardCaptain = 6,
        GuardAdmiral = 7,
        GuardGovernor = 8,
    }
}

internal class InteractParser : IMessageParser
{
    public bool CanParse(string cmd)
    {
        return cmd == "INTERACT_WORD";
    }

    public Task<IMessage> Parse(JObject obj)
    {
        var uid = obj["data"]["uid"].Value<long>();
        var uname = obj["data"]["uname"].Value<string>();
        var timestamp = obj["data"]["timestamp"].Value<long>();
        var type = (Interact.MessageType)obj["data"]["msg_type"].Value<int>();
        var badgeName = (string)obj["data"]["fans_medal"]["medal_name"];
        var badgeLevel = (int)obj["data"]["fans_medal"]["medal_level"];
        var badgeRoomId = (int)obj["data"]["fans_medal"]["anchor_roomid"];
        // var badgeAnchorName = (string)obj["data"]["fans_medal"]["anchor_uname"];
        var badgeUserId = (long)obj["data"]["fans_medal"]["target_id"];
        var badge = badgeUserId == 0 ? null : new Badge(badgeName, badgeLevel);
        var identities = obj["data"]["identities"].Select(x => (Interact.Identity)x.Value<int>()).ToArray();
        var user = new User(Name: uname, Avatar: null, Id: uid);
        var interact = new Interact(user, badge, type, identities.ToImmutableList(), DateTimeOffset.FromUnixTimeSeconds(timestamp));
        return Task.FromResult<IMessage>(interact);
    }
}