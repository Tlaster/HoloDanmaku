using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;
using Newtonsoft.Json;

var bili = new BilibiliApi(310606);
bili.DanmakuReceived += delegate(object sender, DanmakuModel model)
{
    Console.WriteLine(JsonConvert.SerializeObject(model));
};
bili.ViewerCountChanged += delegate(object sender, int u)
{
    Console.WriteLine(u);
};
await bili.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();