using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static Publish DecodePublish(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var flags = buffer[0] & 0x0F;
        var packetSize = buffer.ReadVarInt(ref offset);
        var topicName = buffer.ReadString(ref offset);
        var id = buffer.ReadUInt16(ref offset);
        
        var payload = buffer[offset..].ToArray();
        var qos = (QosLevel)((flags >> 1) & 0x03);

        var packet = new Publish(id, topicName, payload, qos)
        {
            Retain = Convert.ToBoolean(flags & 0x01),
            Dup = Convert.ToBoolean(flags >> 3)
        };

        return packet;
    }
}
