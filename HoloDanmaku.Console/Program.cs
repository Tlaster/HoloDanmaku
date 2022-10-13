using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;

var bili = new BilibiliApi(2577655);
bili.DanmakuReceived += delegate(object sender, DanmakuModel model)
{
    Console.WriteLine(model);
};
bili.ViewerCountChanged += delegate(object sender, uint u)
{
    Console.WriteLine(u);
};
await bili.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();