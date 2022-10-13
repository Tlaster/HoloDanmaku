using System;
using System.Threading.Tasks;
using DanmakuService;
using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MSTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1() 
        {
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

            await Task.Delay(TimeSpan.FromMinutes(3));
        }
    }
}
