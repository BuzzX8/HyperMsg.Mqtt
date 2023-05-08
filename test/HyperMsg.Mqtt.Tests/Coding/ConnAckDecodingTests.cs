using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class ConnAckDecodingTests
{
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
        Assert.Null(connAck.Properties);
    }

    [Fact]
    public void DecodeConnAck_Correctly_Decodes_Properties()
    {
        var encodedPacket = new byte[]
        {
            0x20, 46, //Fixed header
            0x01, //Flags
            0x00, //Reason code
            //Properties
            44, 17, 0, 0, 0, 11, 33, 117, 48, 39, 0, 0, 136, 184, 34, 0, 12, 38, 0, 5, 80, 114, 111, 112, 49, 0, 4, 86, 97, 108, 49, 38, 0, 5, 80, 114, 111, 112, 50, 0, 4, 86, 97, 108, 50,
        };

        var connAck = (ConnAck)Decoding.Decode(encodedPacket, out var _);

        Assert.NotNull(connAck.Properties);
    }
}
