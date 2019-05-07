using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanmakuService.Bilibili.Models;
using DanmakuService.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace DanmakuService.Bilibili
{
    public class BilibiliApi
    {
        private const short ProtocolVersion = 1;
        private readonly int _roomId;
        private readonly WebSocket _socket;
        public event EventHandler<uint> ViewerCountChanged;
        public event EventHandler<DanmakuModel> DanmakuReceived; 

        public BilibiliApi(int roomId)
        {
            _socket = new WebSocket("wss://broadcastlv.chat.bilibili.com/sub");
            _roomId = roomId;
            _socket.OnOpen += OnOpen;
            _socket.OnClose += SocketOnClose;
            _socket.OnMessage += OnMessage;
        }

        public bool IsConnected { get; private set; }

        private void SocketOnClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            var currentIndex = 0;

            var length = e.RawData.SubArray(currentIndex, 4).ToInt32();
            currentIndex += 4;

            if (length < 16) throw new NotSupportedException("failed: (L:" + length + ")");

            var headLength = e.RawData.SubArray(currentIndex, 2).ToInt16();
            currentIndex += 2;
            var shortTag = e.RawData.SubArray(currentIndex, 2).ToInt16();
            currentIndex += 2;
            var type = e.RawData.SubArray(currentIndex, 4).ToInt32();
            currentIndex += 4;
            var tag = e.RawData.SubArray(currentIndex, 4).ToInt32();
            currentIndex += 4;

            var payloadLength = length - 16;
            if (payloadLength == 0) return;
            var buffer = e.RawData.SubArray(currentIndex, payloadLength);
            switch (type)
            {
                case 3:
                {
                    var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
                    ViewerCountChanged?.Invoke(this, viewer);
                    //if (ReceivedRoomCount != null)
                    //{
                    //    ReceivedRoomCount(this, new ReceivedRoomCountArgs() { UserCount = viewer });
                    //}
                    break;
                }

                case 5: //playerCommand
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, payloadLength);
                    try
                    {
                        var obj = JObject.Parse(json);
                        if (obj["cmd"].ToString() == "ROOM_REAL_TIME_MESSAGE_UPDATE")
                        {
                            ViewerCountChanged?.Invoke(this, obj["data"]["fans"].ToObject<uint>());
                        }
                        else
                        {
                            DanmakuReceived?.Invoke(this, new DanmakuModel(obj));
                        }
                    }
                    catch (Exception)
                    {
                    }

                    break;
                }
            }
        }

        public void Start()
        {
            _socket.Connect();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            IsConnected = true;
            SendJoinRoom(_roomId);
            StartSendingHeartbeat();
        }

        private void StartSendingHeartbeat()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (IsConnected)
                    {
                        SendHeartbeat();
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
                catch (Exception e)
                {
                    _socket.Close();
                }
            });
        }

        private void SendHeartbeat()
        {
            SendSocketData(2);
        }

        private void SendSocketData(int action, string body = "")
        {
            SendSocketData(0, 16, ProtocolVersion, action, 1, body);
        }

        private void SendSocketData(int packetLength, short headerLength, short ver, int action, int param = 1,
            string body = "")
        {
            var payload = Encoding.UTF8.GetBytes(body);
            if (packetLength == 0) packetLength = payload.Length + 16;
            var buffer = new byte[packetLength];
            using (var ms = new MemoryStream(buffer))
            {
                var b = BitConverter.GetBytes(buffer.Length).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(headerLength).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(ver).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(action).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(param).ToBE();
                ms.Write(b, 0, 4);
                if (payload.Length > 0) ms.Write(payload, 0, payload.Length);

                _socket.Send(buffer);
            }
        }

        private void SendJoinRoom(int channelId)
        {
            var r = new Random();
            var tmpuid = (long) (1e14 + 2e14 * r.NextDouble());
            var packetModel = new {roomid = channelId, uid = tmpuid};
            var payload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, payload);
        }
    }
}