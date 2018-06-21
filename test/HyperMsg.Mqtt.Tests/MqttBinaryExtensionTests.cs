using System;
using Xunit;

namespace HyperMsg.Mqtt.Tests
{
    public class MqttBinaryExtensionTests
    {
	    [Fact(DisplayName = "WritePacket serializes Unsubscribe packet")]
	    public void WritePacket_Serializes_Unsubscribe_Packet()
	    {
		    ushort packetId = 0x0c1d;
		    var packet = new Unsubscribe(packetId, "a/b", "c/d");
		    byte[] expected =
		    {
			    0b10100010, //Type code
			    12, //Length
			    0x0c, 0x1d, //Packet ID
			    0, 3, 0x61, 0x2f, 0x62, //Filter "a/b"
			    0, 3, 0x63, 0x2f, 0x64	//Filter "c/d"
		    };

		    VerifySerialization(packet, expected);
	    }

		[Fact(DisplayName = "WritePacket UnsubAck packet")]
	    public void WritePacket_Serializes_UnsubAck_Packet()
	    {
		    ushort packetId = 0x0990;
		    VerifySerialization(new UnsubAck(packetId), 0b10110000, 0b00000010, 0x09, 0x90);
	    }

	    [Fact(DisplayName = "WritePacket PingReq packet")]
	    public void WritePacket_Serializes_PingReq_Packet()
	    {
		    VerifySerialization(new PingReq(), 0b11000000, 0b00000000);
	    }

		[Fact(DisplayName = "WritePacket serialzies PingResp packet")]
	    public void WritePacket_Serializes_PingResp_Packet()
	    {
		    VerifySerialization(new PingResp(), 0b11010000, 0b00000000);
	    }

	    [Fact(DisplayName = "WritePacket serialzies Disconnect packet")]
	    public void WritePacket_Serializes_Disconnect_Packet()
	    {
		    VerifySerialization(new Disconnect(), 0b11100000, 0b00000000);
	    }

		private void VerifySerialization(Packet packet, params byte[] expected)
	    {
		    var buffer = new Memory<byte>(new byte[expected.Length]);
		    int written = buffer.WritePacket(packet);

			Assert.Equal(expected.Length, written);
			Assert.Equal(expected, buffer.ToArray());
	    }
    }
}
