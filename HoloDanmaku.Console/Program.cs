using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;
using Newtonsoft.Json;

var bili = new BilibiliApi(264788);
bili.DanmakuReceived += delegate(object sender, IMessage model)
{
    if (model is not RawMessage)
    {
        Console.WriteLine(JsonConvert.SerializeObject(model));
    }
};
await bili.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();