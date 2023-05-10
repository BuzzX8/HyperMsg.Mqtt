using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public static partial class Decoding
{
    private static readonly Dictionary<byte, PropertyUpdater<ConnectProperties>> ConnectPropertyUpdaters = new()
    {
        [0x11] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.SessionExpiryInterval = b.ReadUInt32(ref offset),
        [0x15] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.AuthenticationMethod = b.ReadString(ref offset),
        [0x16] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.AuthenticationData = b.ReadBinaryData(ref offset),
        [0x17] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.RequestProblemInformation = b.ReadBoolean(ref offset),
        [0x19] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.RequestResponseInformation = b.ReadBoolean(ref offset),
        [0x21] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ReceiveMaximum = b.ReadUInt16(ref offset),
        [0x22] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.TopicAliasMaximum = b.ReadUInt16(ref offset),
        [0x26] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) =>
        {
            p.UserProperties ??= new Dictionary<string, string>();
            ReadUserProperty(p.UserProperties, b, ref offset);
        },
        [0x27] = (ConnectProperties p, ReadOnlySpan<byte> b, ref int offset) => p.MaximumPacketSize = b.ReadUInt32(ref offset),
    };

    private static readonly Dictionary<byte, PropertyUpdater<ConnectWillProperties>> ConnectWillPropertyUpdaters = new()
    {
        [0x01] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.PayloadFormatIndicator = b.ReadByte(ref offset),
        [0x02] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.MessageExpiryInterval = b.ReadUInt32(ref offset),
        [0x03] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ContentType = b.ReadString(ref offset),
        [0x08] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.ResponseTopic = b.ReadString(ref offset),
        [0x09] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.CorrelationData = b.ReadBinaryData(ref offset),
        [0x18] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) => p.WillDelayInterval = b.ReadUInt32(ref offset),
        [0x26] = (ConnectWillProperties p, ReadOnlySpan<byte> b, ref int offset) =>
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
