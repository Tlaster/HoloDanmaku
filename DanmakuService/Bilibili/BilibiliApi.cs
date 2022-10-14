using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DanmakuService.Bilibili.Models;
using DanmakuService.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DanmakuService.Bilibili;

public class BilibiliApi
{
    private const short ProtocolVersion = 1;
    private readonly ClientWebSocket _client;
    private readonly int _roomId;
    private readonly CancellationTokenSource _source = new();
    private readonly CancellationToken _token;

    public BilibiliApi(int roomId)
    {
        _roomId = roomId;
        _token = _source.Token;
        _client = new ClientWebSocket();
    }

    public bool IsConnected => _client.State == WebSocketState.Open;
    public event EventHandler<IMessage>? DanmakuReceived;

    private async Task OnMessage(byte[] rawData)
    {
        var protocol = DanmakuProtocol.FromBuffer(rawData);
        if (protocol.PacketLength < protocol.HeaderLength)
        {
            return;
        }

        var payloadLength = protocol.PacketLength - protocol.HeaderLength;
        if (payloadLength == 0)
        {
            return;
        }

        if (protocol.Operation == OperationType.Notification)
        {
            var data = rawData.AsSpan(protocol.HeaderLength, payloadLength).ToArray();
            if (protocol.ProtocolVersion == ProtocolType.Zlib)
            {
                using var compressedStream = new MemoryStream(data);
                await using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                await deflateStream.CopyToAsync(decompressedStream, _token);
                data = decompressedStream.ToArray();
            }
            else if (protocol.ProtocolVersion == ProtocolType.Brotli)
            {
                using var compressedStream = new MemoryStream(data);
                await using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
                using var decompressedStream = new MemoryStream();
                await brotliStream.CopyToAsync(decompressedStream, _token);
                data = decompressedStream.ToArray();
            }
            var json = Encoding.UTF8.GetString(data);
            const string pattern = "[\x00-\x1f]+";
            var items = Regex.Split(json, pattern).Select(it => it.Trim()).Where(it => !string.IsNullOrEmpty(it));
            foreach (var item in items)
            {
                try
                {
                    DanmakuReceived?.Invoke(this, await IMessage.Parse(JObject.Parse(item)));
                }
                catch (Exception e)
                {
                    // Console.WriteLine(e);
                    continue;
                }
            }
        }
        else if (protocol.Operation == OperationType.HeartbeatReply)
        {
            // 房间人气值
            var viewer = BitConverter.ToInt32(rawData, 16);
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
                    var buffer = new byte[4096];
                    var result = await _client.ReceiveAsync(buffer, _token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _token);
                    }
                    else
                    {
                        await OnMessage(buffer);
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
        if (packetLength == 0)
        {
            packetLength = payload.Length + 16;
        }

        var buffer = new byte[packetLength];
        using var ms = new MemoryStream(buffer);
        // BinaryPrimitives.TryWriteInt32BigEndian(buffer.AsSpan(0, 4), buffer.Length);;
        // BinaryPrimitives.TryWriteInt16BigEndian(buffer.AsSpan(4, 2), headerLength);
        // BinaryPrimitives.TryWriteInt16BigEndian(buffer.AsSpan(6, 2), ver);
        // BinaryPrimitives.TryWriteInt32BigEndian(buffer.AsSpan(8, 4), action);
        // BinaryPrimitives.TryWriteInt32BigEndian(buffer.AsSpan(12, 4), param);
        
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
        
        if (payload.Length > 0)
        {
            ms.Write(payload, 0, payload.Length);
        }

        await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, _token);
    }

    private async Task SendJoinRoom(int channelId)
    {
        var packetModel = new { roomid = channelId };
        var payload = JsonConvert.SerializeObject(packetModel);
        await SendSocketData(7, payload);
    }
}

internal record DanmakuProtocol(int PacketLength, short HeaderLength, ProtocolType ProtocolVersion, OperationType Operation, int SequenceId)
{
    public const int HeaderSize = 16;
    internal static DanmakuProtocol FromBuffer(byte[] buffer)
    {
        if (buffer.Length < HeaderSize) { throw new ArgumentException(); }
        var packetLengthData = new byte[4];
        Array.Copy(buffer, 0, packetLengthData, 0, 4);
        var packetLength = BinaryPrimitives.ReadInt32BigEndian(packetLengthData);
        var headerLengthData = new byte[2];
        Array.Copy(buffer, 4, headerLengthData, 0, 2);
        var headerLength = BinaryPrimitives.ReadInt16BigEndian(headerLengthData);
        var protocolVersionData = new byte[2];
        Array.Copy(buffer, 6, protocolVersionData, 0, 2);
        var protocolVersion = BinaryPrimitives.ReadInt16BigEndian(protocolVersionData);
        var operationData = new byte[4];
        Array.Copy(buffer, 8, operationData, 0, 4);
        var operation = BinaryPrimitives.ReadInt32BigEndian(operationData);
        var sequenceIdData = new byte[4];
        Array.Copy(buffer, 12, sequenceIdData, 0, 4);
        var sequenceId = BinaryPrimitives.ReadInt32BigEndian(sequenceIdData);
        return new DanmakuProtocol(packetLength, headerLength, (ProtocolType)protocolVersion, (OperationType)operation, sequenceId);
    }
}

internal enum OperationType
{
    Heartbeat = 2,
    HeartbeatReply = 3,
    Notification = 5,
    JoinRoom = 7,
    JoinRoomReply = 8,
}

internal enum ProtocolType
{
    Json = 0,
    Int32BE = 1,
    Zlib = 2,
    Brotli = 3,
}