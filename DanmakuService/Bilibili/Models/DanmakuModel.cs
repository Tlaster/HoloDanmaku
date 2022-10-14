using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public interface IMessage
{
    static IMessage Parse(JObject obj)
    {
        
    }
}

public record Danmaku(string Content) : IMessage;
