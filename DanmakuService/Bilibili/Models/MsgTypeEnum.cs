namespace DanmakuService.Bilibili.Models
{
    public enum MsgTypeEnum
    {
        /// <summary>
        ///     彈幕
        /// </summary>
        Comment,

        /// <summary>
        ///     禮物
        /// </summary>
        GiftSend,

        /// <summary>
        ///     禮物排名
        /// </summary>
        GiftTop,

        /// <summary>
        ///     歡迎老爷
        /// </summary>
        Welcome,

        /// <summary>
        ///     直播開始
        /// </summary>
        LiveStart,

        /// <summary>
        ///     直播結束
        /// </summary>
        LiveEnd,

        /// <summary>
        ///     其他
        /// </summary>
        Unknown,

        /// <summary>
        ///     欢迎船员
        /// </summary>
        WelcomeGuard,

        /// <summary>
        ///     购买船票（上船）
        /// </summary>
        GuardBuy
    }
}