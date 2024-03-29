﻿using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class PublishDecodingTests
{
    [Fact]
    public void Decode_Correctly_Decodes_Publish_Basic_Fields()
    {
        var buffer = new byte[] {
            0b00111101, //Packet code
            11, //Length
            0, 3, 0x61, 0x2f, 0x62, //Topic name
            0, 10, //Packet ID
            0, //Properties
            9, 8, 7 //Payload
        };

        var packet = Decoding.Decode(buffer, out var _);
        Assert.True(packet.IsPublish);
        var publish = packet.ToPublish();

        Assert.True(publish.Retain);
        Assert.Equal(QosLevel.Qos2, publish.Qos);
        Assert.True(publish.Dup);
        Assert.Equal("a/b", publish.TopicName);
        Assert.Equal(10, publish.Id);
        Assert.Equal(buffer[9..].ToArray(), publish.Payload.ToArray());
        Assert.Null(publish.Properties);
    }

    [Fact]
    public void Decode_Correctly_Decodes_Properties()
    {
        var buffer = new byte[] {
            0b00111101, //Packet code
            21, //Length
            0, 3, 0x61, 0x2f, 0x62, //Topic name
            0, 10, //Packet ID
            //Properties
            10, 0x01, 1, 0x02, 0, 0, 0, 15, 0x23, 0, 8,
            9, 8, 7 //Payload
        };

        var packet = Decoding.Decode(buffer, out var _).ToPublish();

        Assert.NotNull(packet.Properties);
    }
}
