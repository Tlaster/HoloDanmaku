namespace DanmakuService.Bilibili.Models
{
    public class GiftRank
    {
        /// <summary>
        ///     用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     花銷
        /// </summary>
        public decimal coin { get; set; }

        /// <summary>
        ///     UID
        /// </summary>
        public int uid { get; set; }
    }
}