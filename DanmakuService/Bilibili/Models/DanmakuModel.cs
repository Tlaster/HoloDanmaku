using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili.Models
{
    public class DanmakuModel
    {
        public DanmakuModel(JObject obj)
        {
            RawDataJToken = obj;
            var cmd = obj["cmd"].ToString();
            switch (cmd)
            {
                case "LIVE":
                    MsgType = MsgTypeEnum.LiveStart;
                    RoomId = obj["roomid"].ToString();
                    break;
                case "PREPARING":
                    MsgType = MsgTypeEnum.LiveEnd;
                    RoomId = obj["roomid"].ToString();
                    break;
                case "DANMU_MSG":
                    MsgType = MsgTypeEnum.Comment;
                    CommentText = obj["info"][1].ToString();
                    UserId = obj["info"][2][0].ToObject<int>();
                    UserName = obj["info"][2][1].ToString();
                    IsAdmin = obj["info"][2][2].ToString() == "1";
                    IsVip = obj["info"][2][3].ToString() == "1";
                    UserGuardLevel = obj["info"][7].ToObject<int>();
                    break;
                case "SEND_GIFT":
                    MsgType = MsgTypeEnum.GiftSend;
                    GiftName = obj["data"]["giftName"].ToString();
                    UserName = obj["data"]["uname"].ToString();
                    UserId = obj["data"]["uid"].ToObject<int>();
                    // Giftrcost = obj["data"]["rcost"].ToString();
                    GiftCount = obj["data"]["num"].ToObject<int>();
                    break;
                case "GIFT_TOP":
                {
                    MsgType = MsgTypeEnum.GiftTop;
                    var alltop = obj["data"].ToList();
                    GiftRanking = new List<GiftRank>();
                    foreach (var v in alltop)
                        GiftRanking.Add(new GiftRank
                        {
                            uid = v.Value<int>("uid"),
                            UserName = v.Value<string>("uname"),
                            coin = v.Value<decimal>("coin")
                        });
                    break;
                }

                case "WELCOME":
                {
                    MsgType = MsgTypeEnum.Welcome;
                    UserName = obj["data"]["uname"].ToString();
                    UserId = obj["data"]["uid"].ToObject<int>();
                    IsVip = true;
                    IsAdmin = obj["data"]["is_admin"].ToObject<bool>();
                    break;
                }

                case "WELCOME_GUARD":
                {
                    MsgType = MsgTypeEnum.WelcomeGuard;
                    UserName = obj["data"]["username"].ToString();
                    UserId = obj["data"]["uid"].ToObject<int>();
                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                    break;
                }

                case "GUARD_BUY":
                {
                    MsgType = MsgTypeEnum.GuardBuy;
                    UserId = obj["data"]["uid"].ToObject<int>();
                    UserName = obj["data"]["username"].ToString();
                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                    GiftName = UserGuardLevel == 3 ? "舰长" :
                        UserGuardLevel == 2 ? "提督" :
                        UserGuardLevel == 1 ? "总督" : "";
                    GiftCount = obj["data"]["num"].ToObject<int>();
                    break;
                }

                default:
                {
                    MsgType = MsgTypeEnum.Unknown;
                    break;
                }
            }
        }

        /// <summary>
        ///     消息類型
        /// </summary>
        public MsgTypeEnum MsgType { get; }

        /// <summary>
        ///     彈幕內容
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string CommentText { get; }


        /// <summary>
        ///     消息触发者用户名
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string UserName { get; }

        /// <summary>
        ///     消息触发者用户ID
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public int UserId { get; }

        /// <summary>
        ///     用户舰队等级
        ///     <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public int UserGuardLevel { get; }

        /// <summary>
        ///     禮物名稱
        /// </summary>
        public string GiftName { get; }


        /// <summary>
        ///     礼物数量
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>此字段也用于标识上船 <see cref="MsgTypeEnum.GuardBuy" /> 的数量（月数）</para>
        /// </summary>
        public int GiftCount { get; }

        /// <summary>
        ///     禮物排行
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftTop" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public List<GiftRank> GiftRanking { get; }

        /// <summary>
        ///     该用户是否为房管（包括主播）
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool IsAdmin { get; }

        /// <summary>
        ///     是否VIP用戶(老爺)
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool IsVip { get; }

        /// <summary>
        ///     <see cref="MsgTypeEnum.LiveStart" />,<see cref="MsgTypeEnum.LiveEnd" /> 事件对应的房间号
        /// </summary>
        public string RoomId { get; }

        /// <summary>
        ///     原始数据, 高级开发用, 如果需要用原始的JSON数据, 建议使用这个而不是用RawData
        /// </summary>
        public JToken RawDataJToken { get; }
    }
}