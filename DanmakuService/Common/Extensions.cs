using System;
using System.IO;
using System.Linq;
using System.Net;

namespace DanmakuService.Common
{
    internal static class Extensions
    {
        public static byte[] ToBE(this byte[] b)
        {
            return BitConverter.IsLittleEndian ? b.Reverse().ToArray() : b;
        }

        public static int ToInt32(this byte[] value)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(value, 0));
        }

        public static short ToInt16(this byte[] value)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(value, 0));
        }

        public static void ReadB(this MemoryStream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            var read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0) throw new ObjectDisposedException(null);
                if (available != count) throw new NotSupportedException();
                read += available;
                offset += available;
            }
        }
    }
}