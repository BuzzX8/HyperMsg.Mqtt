using HyperMsg.Coding;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static readonly Dictionary<byte, PropertyUpdater<ConnectProperties>> ConnectPropertyUpdaters = new()
    {
        [0x11] = (p, b, ref offset) => p.SessionExpiryInterval = b.ReadUInt32(ref offset),
        [0x15] = (p, b, ref offset) => p.AuthenticationMethod = b.ReadString(ref offset),
        [0x16] = (p, b, ref offset) => p.AuthenticationData = b.ReadBinaryData(ref offset),
        [0x17] = (p, b, ref offset) => p.RequestProblemInformation = b.ReadBoolean(ref offset),
        [0x19] = (p, b, ref offset) => p.RequestResponseInformation = b.ReadBoolean(ref offset),
        [0x21] = (p, b, ref offset) => p.ReceiveMaximum = b.ReadUInt16(ref offset),
        [0x22] = (p, b, ref offset) => p.TopicAliasMaximum = b.ReadUInt16(ref offset),
        [0x26] = (p, b, ref offset) =>
        {
            p.UserProperties ??= new Dictionary<string, string>();
            ReadUserProperty(p.UserProperties, b, ref offset);
        },
        [0x27] = (p, b, ref offset) => p.MaximumPacketSize = b.ReadUInt32(ref offset),
    };

    private static readonly Dictionary<byte, PropertyUpdater<ConnectWillProperties>> ConnectWillPropertyUpdaters = new()
    {
        [0x01] = (p, b, ref offset) => p.PayloadFormatIndicator = b.ReadByte(ref offset),
        [0x02] = (p, b, ref offset) => p.MessageExpiryInterval = b.ReadUInt32(ref offset),
        [0x03] = (p, b, ref offset) => p.ContentType = b.ReadString(ref offset),
        [0x08] = (p, b, ref offset) => p.ResponseTopic = b.ReadString(ref offset),
        [0x09] = (p, b, ref offset) => p.CorrelationData = b.ReadBinaryData(ref offset),
        [0x18] = (p, b, ref offset) => p.WillDelayInterval = b.ReadUInt32(ref offset),
        [0x26] = (p, b, ref offset) =>
        {
            p.UserProperties ??= new Dictionary<string, string>();
            ReadUserProperty(p.UserProperties, b, ref offset);
        }
    };

    private static Connect DecodeConnect(ReadOnlySpan<byte> buffer)
    {
        var offset = 1;
        var packetSize = buffer.ReadVarInt(ref offset);

        //Variable header
        VerifyProtocolName(buffer, ref offset);
        var protocolVersion = buffer.ReadByte(ref offset);
        var flags = (ConnectFlags)buffer.ReadByte(ref offset);
        var keepAlive = buffer.ReadUInt16(ref offset);
        var properties = DecodeConnectProperties(buffer, protocolVersion, ref offset);

        //Payload
        var clientId = buffer.ReadString(ref offset);

        var connect = Connect.NewV5(clientId);
        connect.Flags = flags;
        connect.KeepAlive = keepAlive;
        connect.Properties = properties;

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

    private static void VerifyProtocolName(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var protocolName = buffer.ReadString(ref offset);
        if (protocolName != "MQTT")
        {
            throw new DecodingException("Invalid protocol name");
        }
    }

    private static ConnectProperties DecodeConnectProperties(ReadOnlySpan<byte> buffer, byte protocolVersion, ref int offset)
    {
        if (protocolVersion < 5)
        {
            return default;
        }

        return DecodeProperties(buffer, ConnectPropertyUpdaters, ref offset);
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

        return DecodeProperties(buffer, ConnectWillPropertyUpdaters, ref offset);
    }
}
