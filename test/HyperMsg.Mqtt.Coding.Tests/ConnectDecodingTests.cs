using HyperMsg.Coding;
using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public class ConnectDecodingTests
{
    [Fact]
    public void DecodeConnect_Correctly_Decodes_Variable_Header()
    {
        var packet = Decoding.Decode(ConnectDecodingTestData.EncodedConnectPacket, out var _);
        Assert.True(packet.IsConnect);
        Assert.Equal(PacketKind.Connect, packet.Kind);
        var connect = packet.ToConnect();

        Assert.Equal("MQTT", connect.ProtocolName);
        Assert.Equal(5, connect.ProtocolVersion);
        Assert.Equal(60, connect.KeepAlive);

        Assert.Equal(11u, connect.Properties.SessionExpiryInterval);
        Assert.Equal(30000u, connect.Properties.ReceiveMaximum);
        Assert.Equal(35000u, connect.Properties.MaximumPacketSize);
        Assert.Equal(12u, connect.Properties.TopicAliasMaximum);
        Assert.True(connect.Properties.RequestResponseInformation);
        Assert.True(connect.Properties.RequestProblemInformation);
        Assert.Equal("Val1", connect.Properties.UserProperties["Prop1"]);
        Assert.Equal("Val2", connect.Properties.UserProperties["Prop2"]);
    }

    [Fact]
    public void DecodeConnect_Correctly_Decodes_Payload()
    {
        var packet = Decoding.Decode(ConnectDecodingTestData.EncodedConnectPacket, out var _).ToConnect();

        Assert.Equal("mqttx_4b94267f", packet.ClientId);
        Assert.Equal("last-will-topic", packet.WillTopic);
        Assert.Equal("John Doe", packet.UserName);
    }

    [Fact]
    public void DecodeConnect_Invalid_Protocol_Name_Throws_Exception()
    {
        Assert.Throws<DecodingException>(() => Decoding.Decode(ConnectDecodingTestData.InvalidProtocolName, out var _));
    }
}
