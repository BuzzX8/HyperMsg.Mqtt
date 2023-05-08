using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    public static ConnAck DecodeConnAck(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var length = buffer.ReadVarInt(ref offset);

        var flags = buffer.ReadByte(ref offset);
        var sessionPresent = Convert.ToBoolean(flags & 0x01);
        var reasonCode = (ConnectReasonCode)buffer.ReadByte(ref offset);

        return new(reasonCode, sessionPresent);
    }
}
