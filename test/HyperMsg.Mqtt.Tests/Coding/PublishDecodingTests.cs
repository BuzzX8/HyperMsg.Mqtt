using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class PublishDecodingTests
{
    [Fact]
    public void Decode_Publish_Correctly_Decodes_Basic_Fields()
    {
        var buffer = new byte[] {
            0b00111101, //Packet code
            10, //Length
            0, 3, 0x61, 0x2f, 0x62, //Topic name
            0, 10, //Packet ID
            9, 8, 7 //Payload
        };

        var packet = (Publish)Decoding.Decode(buffer, out var _);

        Assert.True(packet.Retain);
        Assert.Equal(QosLevel.Qos2, packet.Qos);
        Assert.True(packet.Dup);
        Assert.Equal("a/b", packet.TopicName);
        Assert.Equal(10, packet.Id);
        Assert.Equal(buffer[9..].ToArray(), packet.Payload.ToArray());
    }
}
