using System;

namespace HyperMsg.Mqtt
{
    internal static class PacketId
    {
		public static ushort New() => BitConverter.ToUInt16(Guid.NewGuid().ToByteArray(), 0);
    }
}