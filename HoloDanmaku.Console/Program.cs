using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;
using Newtonsoft.Json;

var bili = new BilibiliApi(13);
bili.MessageReceived += delegate(object sender, IMessage model)
{
    if (model is not RawMessage)
    {
        Console.WriteLine(model.GetType().Name + ": " + JsonConvert.SerializeObject(model));
    }
};
await bili.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();