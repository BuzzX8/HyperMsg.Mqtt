using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class SubscribeDecodingTests
{
    [Fact]
    public void Decode_Subscribe_()
    {
        var encodedPacket = new byte[]
        {
            0b10000010, //Packet code
            14, //Length
            0, 4, //Packet ID
            0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b", Qos1
            0, 3, 0x63, 0x2f, 0x64, 2 //Filter "c/d", Qos2
        };

        var packet = Decoding.Decode(encodedPacket, out var _);
        Assert.True(packet.IsSubscribe);
        var subscribe = packet.ToSubscribe();
    }
}
