using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;
using Newtonsoft.Json;

var bili = new BilibiliApi(21452505);
bili.DanmakuReceived += delegate(object sender, IMessage model)
{
    Console.WriteLine(JsonConvert.SerializeObject(model));
};
await bili.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();