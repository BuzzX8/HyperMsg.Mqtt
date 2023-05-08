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
        var properties = DecodeConnAckProperties(buffer, ref offset);

        return new(reasonCode, sessionPresent, properties);
    }

    private static ConnAckProperties DecodeConnAckProperties(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var propLength = buffer.ReadVarInt(ref offset);

        if (propLength == 0)
        {
            return default;
        }

        var properties = new ConnAckProperties();
        var propBuffer = buffer[offset..(offset + propLength)];

        ReadConnAckProperties(properties, propBuffer);

        return properties;
    }

    private static void ReadConnAckProperties(ConnAckProperties properties, ReadOnlySpan<byte> propBuffer) 
    {
        var offset = 0;

        while (offset < propBuffer.Length)
        {
            var propCode = propBuffer.ReadByte(ref offset);

            ReadConnAckProperty(properties, propCode, propBuffer, ref offset);
        }
    }

    private static void ReadConnAckProperty(ConnAckProperties properties, byte propCode, ReadOnlySpan<byte> buffer, ref int offset)
    {
        switch (propCode)
        {
            case 0x11:
                properties.SessionExpiryInterval = buffer.ReadUInt32(ref offset); 
                break;

            case 0x12:
                properties.AssignedClientIdentifier = buffer.ReadString(ref offset);
                break;

            case 0x13:
                properties.ServerKeepAlive = buffer.ReadUInt16(ref offset);
                break;

            case 0x15:
                properties.AuthenticationMethod = buffer.ReadString(ref offset);
                break;

            case 0x16:
                properties.AuthenticationData = buffer.ReadBinaryData(ref offset);
                break;

            case 0x1A:
                properties.ResponseInformation = buffer.ReadString(ref offset);
                break;

            case 0x1C:
                properties.ServerReference = buffer.ReadString(ref offset);
                break;

            case 0x1F:
                properties.ReasonString = buffer.ReadString(ref offset);
                break;

            case 0x21:
                properties.ReceiveMaximum = buffer.ReadUInt16(ref offset);
                break;

            case 0x22:
                properties.TopicAliasMaximum = buffer.ReadUInt16(ref offset);
                break;

            case 0x24:
                properties.MaximumQos = buffer.ReadByte(ref offset);
                break;

            case 0x25:
                properties.RetainAvailable = buffer.ReadBoolean(ref offset);
                break;

            case 0x26:
                properties.UserProperties ??= new Dictionary<string, string>();
                ReadUserProperty(properties.UserProperties, buffer, ref offset);
                break;

            case 0x27:
                properties.MaximumPacketSize = buffer.ReadUInt32(ref offset);
                break;

            case 0x28:
                properties.WildcardSubscriptionAvailable = buffer.ReadBoolean(ref offset);
                break;

            case 0x29:
                properties.SubscriptionIdentifierAvailable = buffer.ReadBoolean(ref offset);
                break;

            case 0x2A:
                properties.SharedSubscriptionAvailable = buffer.ReadBoolean(ref offset);
                break;

            default:
                throw new DecodingError($"Incorrect ConnAck property code provided ({propCode})");
        }
    }
}
