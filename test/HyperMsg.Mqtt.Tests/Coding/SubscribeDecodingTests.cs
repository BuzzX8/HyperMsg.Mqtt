using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class SubscribeDecodingTests
{
    [Fact]
    public void Decode_Subscribe_Correctly_Decodes_Id_And_Requests()
    {
        var encodedPacket = new byte[]
        {
            0b10000010, //Packet code
            14, //Length
            0, 4, //Packet ID
            0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b", Qos1
            0, 3, 0x63, 0x2f, 0x64, 2, //Filter "c/d", Qos2,
            0
        };

        var packet = Decoding.Decode(encodedPacket, out var _);
        Assert.True(packet.IsSubscribe);
        var subscribe = packet.ToSubscribe();

        Assert.Equal(4, subscribe.Id);
        Assert.Equal(2, subscribe.Requests.Count);
        Assert.Null(subscribe.Properties);
    }
}
