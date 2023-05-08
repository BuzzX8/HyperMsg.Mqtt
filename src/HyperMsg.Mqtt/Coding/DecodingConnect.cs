using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    public static Connect DecodeConnect(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var packetSize = buffer.ReadVarInt(ref offset);

        //Variable header
        var protocolName = buffer.ReadString(ref offset);
        var protocolVersion = buffer.ReadByte(ref offset);
        var flags = (ConnectFlags)buffer.ReadByte(ref offset);
        var keepAlive = buffer.ReadUInt16(ref offset);
        var properties = DecodeConnectProperties(buffer, protocolVersion, ref offset);

        //Payload
        var clientId = buffer.ReadString(ref offset);
        var connect = new Connect
        {
            ProtocolName = protocolName,
            ProtocolVersion = protocolVersion,
            Flags = flags,
            KeepAlive = keepAlive,
            ClientId = clientId,
            Properties = properties
        };

        if (flags.HasFlag(ConnectFlags.Will))
        {
            ReadWillFields(connect, buffer, ref offset);
        }

        if (flags.HasFlag(ConnectFlags.UserName))
        {
            connect.UserName = buffer.ReadString(ref offset);
        }

        if (flags.HasFlag(ConnectFlags.Password))
        {
            connect.Password = buffer.ReadBinaryData(ref offset);
        }

        return connect;
    }

    private static ConnectProperties DecodeConnectProperties(ReadOnlySpan<byte> buffer, byte protocolVersion, ref int offset)
    {
        if (protocolVersion < 5)
        {
            return default;
        }

        var propLength = buffer.ReadVarInt(ref offset);

        if (propLength == 0)
        {
            return default;
        }

        var properties = new ConnectProperties();
        var propBuffer = buffer[offset..(offset + propLength)];

        ReadConnectProperties(properties, propBuffer);

        offset += propLength;

        return properties;
    }

    private static void ReadConnectProperties(ConnectProperties properties, ReadOnlySpan<byte> propBuffer)
    {
        var offset = 0;

        while (offset < propBuffer.Length)
        {
            var propCode = propBuffer.ReadByte(ref offset);

            ReadConnectProperty(properties, propCode, propBuffer, ref offset);
        }
    }

    private static void ReadConnectProperty(ConnectProperties properties, byte propCode, ReadOnlySpan<byte> buffer, ref int offset)
    {
        switch (propCode)
        {
            case 0x11:
                properties.SessionExpiryInterval = buffer.ReadUInt32(ref offset);
                break;

            case 0x15:
                properties.AuthenticationMethod = buffer.ReadString(ref offset);
                break;

            case 0x16:
                properties.AuthenticationData = buffer.ReadBinaryData(ref offset);
                break;

            case 0x17:
                properties.RequestProblemInformation = Convert.ToBoolean(buffer.ReadByte(ref offset));
                break;

            case 0x19:
                properties.RequestResponseInformation = Convert.ToBoolean(buffer.ReadByte(ref offset));
                break;

            case 0x21:
                properties.ReceiveMaximum = buffer.ReadUInt16(ref offset);
                break;

            case 0x22:
                properties.TopicAliasMaximum = buffer.ReadUInt16(ref offset);
                break;

            case 0x26:
                properties.UserProperties ??= new Dictionary<string, string>();
                ReadUserProperty(properties.UserProperties, buffer, ref offset);
                break;

            case 0x27:
                properties.MaximumPacketSize = buffer.ReadUInt32(ref offset);
                break;

            default:
                throw new DecodingError($"Incorrect connect property code provided ({propCode})");
        }
    }

    private static void ReadWillFields(Connect connect, ReadOnlySpan<byte> buffer, ref int offset)
    {
        connect.WillProperties = DecodeWillProperties(buffer, connect.ProtocolVersion, ref offset);
        connect.WillTopic = buffer.ReadString(ref offset);
        connect.WillPayload = buffer.ReadBinaryData(ref offset);
    }

    private static ConnectWillProperties DecodeWillProperties(ReadOnlySpan<byte> buffer, byte protocolVersion, ref int offset)
    {
        if (protocolVersion < 5)
        {
            return default;
        }

        var willPropLength = buffer.ReadVarInt(ref offset);

        if (willPropLength == 0)
        {
            return null;
        }

        var props = new ConnectWillProperties();
        var propBuffer = buffer[offset..(offset + willPropLength)];

        ReadWillProperties(props, propBuffer);

        offset += willPropLength;

        return props;
    }

    private static void ReadWillProperties(ConnectWillProperties properties, ReadOnlySpan<byte> propBuffer)
    {
        var offset = 0;

        while (offset < propBuffer.Length)
        {
            var propCode = propBuffer.ReadByte(ref offset);

            ReadWillProperty(properties, propCode, propBuffer, ref offset);
        }
    }

    private static void ReadWillProperty(ConnectWillProperties properties, byte propCode, ReadOnlySpan<byte> propBuffer, ref int offset)
    {
        switch (propCode)
        {
            case 0x01:
                properties.PayloadFormatIndicator = propBuffer.ReadByte(ref offset);
                break;

            case 0x02:
                properties.MessageExpiryInterval = propBuffer.ReadUInt32(ref offset);
                break;

            case 0x03:
                properties.ContentType = propBuffer.ReadString(ref offset);
                break;

            case 0x08:
                properties.ResponseTopic = propBuffer.ReadString(ref offset);
                break;

            case 0x09:
                properties.CorrelationData = propBuffer.ReadBinaryData(ref offset);
                break;

            case 0x18:
                properties.WillDelayInterval = propBuffer.ReadUInt32(ref offset);
                break;

            case 0x26:
                properties.UserProperties ??= new Dictionary<string, string>();
                ReadUserProperty(properties.UserProperties, propBuffer, ref offset);
                break;

            default:
                throw new DecodingError($"Incorrect property code for last will property ({propCode})");
        }
    }
}
