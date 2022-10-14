using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models;

public interface IMessage
{
    private static readonly ImmutableList<IMessageParser> Parsers;
    static IMessage()
    {
        // get Parsers by reflection
        Parsers = typeof(IMessage).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IMessageParser).IsAssignableFrom(t))
            .Select(t => Activator.CreateInstance(t) as IMessageParser)
            .Where(p => p != null)
            .Select(p => p!)
            .ToImmutableList();
    }
    
    DateTimeOffset Timestamp { get; }
    static async Task<IMessage> Parse(JObject obj)
    {
        var cmd = obj["cmd"].Value<string>();
        foreach (var parser in Parsers)
        {
            if (parser.CanParse(cmd))
            {
                return await parser.Parse(obj);
            }
        }

        return new RawMessage(obj);
    }
}

public record RawMessage(JObject Object) : IMessage
{
    public DateTimeOffset Timestamp => DateTimeOffset.Now;
}

internal interface IMessageParser
{
    bool CanParse(string cmd);
    Task<IMessage> Parse(JObject obj);
}