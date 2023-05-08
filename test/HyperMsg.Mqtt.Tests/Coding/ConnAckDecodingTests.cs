using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class ConnAckDecodingTests
{
    private static readonly byte[] EncodedConnAck = 
    {
    };

    [Fact]
    public void DecodeConnAck_Correctly_Decodes_Flags_And_ReasonCode()
    {
        var sessionPresent = true;
        var reasonCode = ConnectReasonCode.Success;
        var encodedPacket = new byte[] 
        {
            0x20, 0x03, //Fixed header
            Convert.ToByte(sessionPresent), (byte)reasonCode, 0x00
        };

        var connAck = (ConnAck)Decoding.Decode(encodedPacket, out var _);

        Assert.Equal(sessionPresent, connAck.SessionPresent);
        Assert.Equal(reasonCode, connAck.ReasonCode);
    }
}
