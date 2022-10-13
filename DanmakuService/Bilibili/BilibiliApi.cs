using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DanmakuService.Bilibili.Models;
using DanmakuService.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili
{
    public class BilibiliApi
    {
        private const short ProtocolVersion = 1;
        private readonly int _roomId;
        private readonly ClientWebSocket _client;
        private readonly CancellationTokenSource _source = new();
        private readonly CancellationToken _token;
        public event EventHandler<uint> ViewerCountChanged;
        public event EventHandler<DanmakuModel> DanmakuReceived; 

        public BilibiliApi(int roomId)
        {
            _roomId = roomId;
            _token = _source.Token;
            _client = new ClientWebSocket();
        }

        public bool IsConnected => _client.State == WebSocketState.Open;

        private void OnMessage(byte[] rawData)
        {
            var currentIndex = 0;

            var length = rawData.Skip(currentIndex).Take(4).ToArray().ToInt32();
            currentIndex += 4;

            Console.WriteLine("length: " + length);
            if (length < 16)
            {
                throw new NotSupportedException("failed: (L:" + length + ")");
            }

            var headLength = rawData.Skip(currentIndex).Take(2).ToArray().ToInt16();
            currentIndex += 2;
            var dataType = rawData.Skip(currentIndex).Take(2).ToArray().ToInt16();
            currentIndex += 2;
            var type = rawData.Skip(currentIndex).Take(4).ToArray().ToInt32();
            currentIndex += 4;
            var tag = rawData.Skip(currentIndex).Take(4).ToArray().ToInt32();
            currentIndex += 4;

            var payloadLength = length - 16;
            if (payloadLength == 0) return;
            var buffer = rawData.Skip(currentIndex).Take(payloadLength).ToArray();
            switch (dataType)
            {
                case 2:
                    using (var compressedStream = new MemoryStream(rawData, currentIndex, payloadLength))
                    {
                        using (var gZipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                        {
                            using (var decompressedStream = new MemoryStream())
                            {
                                gZipStream.CopyTo(decompressedStream);
                                decompressedStream.Position = 0;
                                buffer = decompressedStream.ToArray();
                            }
                        }
                    }

                    break;
            }
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    break;
                }
            }
        }

        public async Task StartAsync()
        {
            await _client.ConnectAsync(new Uri("wss://broadcastlv.chat.bilibili.com/sub"), _token);
            await OnOpen();
            StartMessageLoop();
        }

        private void StartMessageLoop()
        {
            Task.Run(async () =>
            {
                while (IsConnected)
                {
                    try
                    {
                        var buffer = new ArraySegment<byte>(new byte[4096]);
                        var result = await _client.ReceiveAsync(buffer, _token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _token);
                        }
                        else
                        {
                            OnMessage(buffer.Array);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }, _token);
        }

        private async Task OnOpen()
        {
            await SendJoinRoom(_roomId);
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
                        await SendHeartbeat();
                        await Task.Delay(TimeSpan.FromSeconds(30), _token);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await _client.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, _token);
                }
            }, _token);
        }

        private async Task SendHeartbeat()
        {
            await SendSocketData(2);
        }

        private async Task SendSocketData(int action, string body = "")
        {
            await SendSocketData(0, 16, ProtocolVersion, action, 1, body);
        }

        private async Task SendSocketData(int packetLength, short headerLength, short ver, int action, int param = 1,
            string body = "")
        {
            var payload = Encoding.UTF8.GetBytes(body);
            if (packetLength == 0) packetLength = payload.Length + 16;
            var buffer = new byte[packetLength];
            using var ms = new MemoryStream(buffer);
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
            await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, _token);
        }

        private async Task SendJoinRoom(int channelId)
        {
            var packetModel = new {roomid = channelId};
            var payload = JsonConvert.SerializeObject(packetModel);
            await SendSocketData(7, payload);
        }
    }
}