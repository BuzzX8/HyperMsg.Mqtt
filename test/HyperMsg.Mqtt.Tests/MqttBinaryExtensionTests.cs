using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace HyperMsg.Mqtt.Tests
{
    public class MqttBinaryExtensionTests
    {
	    [Fact(DisplayName = "WritePacket serializes PubAck packet")]
	    public void WritePacket_Serializes_Puback_Packet()
	    {
		    ushort packetId = 0x8178;
		    var packet = new PubAck(packetId);
		    byte[] expected =
		    {
			    0b01000000, //Type code
			    2, //Length
			    0x81, 0x78 //Packet ID
		    };

			VerifySerialization(packet, expected);
		}

	    [Fact(DisplayName = "WritePacket serializes PubRec packet")]
	    public void WritePacket_Serializes_Pubrec_Packet()
	    {
		    ushort packetId = 0x8179;
		    var packet = new PubRec(packetId);
		    byte[] expected =
		    {
			    0b01010000, //Type code
			    2, //Length
			    0x81, 0x79 //Packet ID
		    };

			VerifySerialization(packet, expected);
		}

	    [Fact(DisplayName = "WritePacket serializes PubRel packet")]
	    public void WritePacket_Serializes_Pubrel_Packet()
	    {
		    ushort packetId = 0x8079;
		    var packet = new PubRel(packetId);
		    byte[] expected =
		    {
			    0b01100010, //Type code
			    2, //Length
			    0x80, 0x79 //Packet ID
		    };

			VerifySerialization(packet, expected);
		}

		[Fact(DisplayName = "WritePacket serialzies PubComp packet")]
	    public void WritePacket_Serializes_Pubcomp_Packet()
	    {
		    ushort packetId = 0x8989;
		    var packet = new PubComp(packetId);
		    byte[] expected =
		    {
			    0b01110000, //Type code
			    2, //Length
			    0x89, 0x89 //Packet ID
		    };

		    VerifySerialization(packet, expected);
	    }

		[Fact(DisplayName = "WritePacket serializes Subscribe packet")]
	    public void WritePacket_Serializes_Subscribe_Packet()
	    {
		    ushort packetId = 0x8098;
		    var packet = new Subscribe(packetId, ("a/b", QosLevel.Qos1), ("c/d", QosLevel.Qos2));
		    byte[] expected =
		    {
			    0b10000010, //Type code
			    14, //Length
			    0x80, 0x98, //Packet ID
			    0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b" Qos1
			    0, 3, 0x63, 0x2f, 0x64, 2 //Filter "c/d" Qos2
		    };

		    VerifySerialization(packet, expected);
	    }

		[Fact(DisplayName = "WritePacket serializes SubAck packet")]
	    public void WritePacket_Serializes_SubAck_Packet()
	    {
		    ushort packetId = 0x6790;
		    var packet = new SubAck(packetId, SubscriptionResult.SuccessQos0, SubscriptionResult.SuccessQos2, SubscriptionResult.Failure);
		    byte[] expected =
		    {
			    0b10010000, //Type code
			    5, //Length
			    0x67, 0x90, //Packet ID
			    0, 0x02, 0x80 //Response codes
		    };

		    VerifySerialization(packet, expected);
	    }

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

		    Assert.Equal(expected, buffer.ToArray());
		    Assert.Equal(expected.Length, written);
	    }

	    public static IEnumerable<object[]> GetTestCasesForWriteRemaningLength()
	    {
		    yield return GetTestCaseForWriteRemainingLength(0, 0);
		    yield return GetTestCaseForWriteRemainingLength(127, 0x7f);
		    yield return GetTestCaseForWriteRemainingLength(128, 0x80, 0x01);
		    yield return GetTestCaseForWriteRemainingLength(16383, 0xFF, 0x7F);
		    yield return GetTestCaseForWriteRemainingLength(16384, 0x80, 0x80, 0x01);
		    yield return GetTestCaseForWriteRemainingLength(2097151, 0xFF, 0xFF, 0x7F);
		    yield return GetTestCaseForWriteRemainingLength(2097152, 0x80, 0x80, 0x80, 0x01);
		    yield return GetTestCaseForWriteRemainingLength(268435455, 0xFF, 0xFF, 0xFF, 0x7F);
	    }

	    public static object[] GetTestCaseForWriteRemainingLength(int value, params byte[] expected)
	    {
		    return new object[] { value, expected };
	    }

	    [Theory(DisplayName = "WriteRemainingLength serializes value for remaining length")]
	    [MemberData(nameof(GetTestCasesForWriteRemaningLength))]
	    public void WriteRemainingLength_Serializes_Value_For_Remaining_Length(int value, byte[] expected)
	    {
		    var buffer = new Memory<byte>(new byte[expected.Length]);

		    int bytesWritten = buffer.WriteRemainingLength(value, 0);

		    Assert.Equal(expected.Length, bytesWritten);
		    Assert.Equal(expected, buffer.ToArray());
	    }

	    [Fact(DisplayName = "WriteString correctly serializes string")]
	    public void WriteString_Correctly_Serializes_String()
	    {
		    string value = Guid.NewGuid().ToString();
		    byte[] expected = new byte[] { 0, (byte)value.Length }.Concat(Encoding.UTF8.GetBytes(value)).ToArray();
			var buffer = new Memory<byte>(expected);

		    var bytesWritten = buffer.WriteString(value, 0);
		    byte[] actual = buffer.ToArray();

			Assert.Equal(expected.Length, bytesWritten);
		    Assert.Equal(expected, actual);
	    }
	}
}
