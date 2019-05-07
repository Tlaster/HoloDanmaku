using System;
using System.Threading.Tasks;
using DanmakuService;
using DanmakuService.Bilibili;
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
            bili.Start();

            await Task.Delay(TimeSpan.FromMinutes(3));
        }
    }
}
