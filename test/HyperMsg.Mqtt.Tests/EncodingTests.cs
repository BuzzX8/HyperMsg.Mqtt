using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace HyperMsg.Mqtt
{
    public class EncodingTests
    {		
		private readonly Buffer buffer;
		private readonly IBufferWriter bufferWriter;

		public EncodingTests()
        {
			var memoryOwner = MemoryPool<byte>.Shared.Rent();
			buffer = new Buffer(memoryOwner);
			bufferWriter = buffer.Writer;
        }

		[Fact(DisplayName = "Serializes Connect packet with Password flag")]
		public void Serializes_Connect_With_Password_Flag()
		{
			byte[] password = Guid.NewGuid().ToByteArray();
			var packet = new Connect
			{
				Flags = ConnectFlags.Password,
				ClientId = Guid.NewGuid().ToString(),
				Password = password
			};
			var expected = CreateConnectHeader(packet);
			AddBinary(expected, password);
			SetRemainingLength(expected);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected.ToArray());
		}

		[Fact(DisplayName = "Serializes Connect with Username flag")]
		public void Serializes_Connect_With_Username_Flag()
		{
			string username = Guid.NewGuid().ToString();
			var packet = new Connect
			{
				ClientId = Guid.NewGuid().ToString(),
				Flags = ConnectFlags.UserName,
				UserName = username
			};
			var expected = CreateConnectHeader(packet);
			AddString(expected, username);
			SetRemainingLength(expected);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected.ToArray());
		}

		[Fact(DisplayName = "Serializes Connect with Will flag")]
		public void Serializes_Connect_With_Will_Flag()
		{
			string willTopic = Guid.NewGuid().ToString();
			byte[] willMessage = Guid.NewGuid().ToByteArray();
			var packet = new Connect
			{
				ClientId = Guid.NewGuid().ToString(),
				Flags = ConnectFlags.Will,
				WillTopic = willTopic,
				WillMessage = willMessage
			};
			var expected = CreateConnectHeader(packet);
			AddString(expected, willTopic);
			AddBinary(expected, willMessage);
			SetRemainingLength(expected);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected.ToArray());
		}

		public static IEnumerable<object[]> GetTestCasesForSerializeConnect()
		{
			yield return new object[] { ConnectFlags.None };
			yield return new object[] { ConnectFlags.CleanSession };
			yield return new object[] { ConnectFlags.WillRetain };
			yield return new object[] { ConnectFlags.Qos0 };
			yield return new object[] { ConnectFlags.Qos1 };
			yield return new object[] { ConnectFlags.Qos2 };
			yield return new object[] { ConnectFlags.CleanSession | ConnectFlags.WillRetain };
		}

		[Theory(DisplayName = "Serializes Connect packet")]
		[MemberData(nameof(GetTestCasesForSerializeConnect))]
		public void Serializes_Connect(ConnectFlags flags)
		{
			ushort keepAlive = BitConverter.ToUInt16(Guid.NewGuid().ToByteArray(), 0);
			string clientId = Guid.NewGuid().ToString();
			var packet = new Connect
			{
				Flags = flags,
				KeepAlive = keepAlive,
				ClientId = clientId
			};
			var expected = CreateConnectHeader(packet);
			SetRemainingLength(expected);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected.ToArray());
		}

		private static List<byte> CreateConnectHeader(Connect packet)
		{
			List<byte> expected = new List<byte>
			{
				0x10, //Type code
				0, //Length placeholder
				0, 4, (byte)'M', (byte)'Q', (byte)'T', (byte)'T', //Protocol name
				4, //Protocol level
				(byte)packet.Flags, //Flags
				(byte)(packet.KeepAlive >> 8), (byte)packet.KeepAlive,
				//0, (byte)packet.ClientId.Length//Client ID length
			};
			AddString(expected, packet.ClientId);
			return expected;
		}

		private static void AddBinary(List<byte> packet, byte[] bytes)
		{
			packet.AddRange(new byte[] { 0, (byte)bytes.Length });
			packet.AddRange(bytes);
		}

		private static void AddString(List<byte> packet, string str)
		{
			packet.AddRange(new byte[] { 0, (byte)str.Length });
			packet.AddRange(System.Text.Encoding.UTF8.GetBytes(str));
		}

		private static void SetRemainingLength(List<byte> packet)
		{
			packet[1] = (byte)(packet.Count - 2);
		}

		public static IEnumerable<object[]> GetTestCasesForSerializeConnAck()
		{
			yield return new object[] { false, ConnectionResult.Accepted };
			yield return new object[] { true, ConnectionResult.BadUsernameOrPassword };
			yield return new object[] { false, ConnectionResult.IdentifierRejected };
			yield return new object[] { true, ConnectionResult.NotAuthorized };
			yield return new object[] { false, ConnectionResult.ServerUnavailable };
			yield return new object[] { true, ConnectionResult.UnacceptableVersion };
		}

		[Theory(DisplayName = "Serializes ConnAck packet")]
		[MemberData(nameof(GetTestCasesForSerializeConnAck))]
		public void Serializes_ConnAck_Packet(bool sessionPresent, ConnectionResult result)
		{
			var packet = new ConnAck(result, sessionPresent);
			byte[] expected =
			{
				0b00100000, //packet code
				0x02, //length
				(byte)(sessionPresent ? 0x01 : 0x00), //flags
				(byte)result
			};

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		public static IEnumerable<object[]> GetTestCasesForSerializePublish()
		{
			yield return new object[] { 0b00110000, false, QosLevel.Qos0, false };
			yield return new object[] { 0b00111000, true, QosLevel.Qos0, false };
			yield return new object[] { 0b00111010, true, QosLevel.Qos1, false };
			yield return new object[] { 0b00111100, true, QosLevel.Qos2, false };
			yield return new object[] { 0b00111011, true, QosLevel.Qos1, true };
		}

		[Theory(DisplayName = "Serializes Publish packet")]
		[MemberData(nameof(GetTestCasesForSerializePublish))]
		public void Serializes_Publish_Packet(byte expectedHeader, bool dup, QosLevel qos, bool retain)
		{
			ushort packetId = 0x8667;
			string topicName = Guid.NewGuid().ToString();
			byte[] payload = Guid.NewGuid().ToByteArray();

			var packet = new Publish(packetId, topicName, payload, qos)
			{
				Dup = dup,
				Retain = retain
			};
			var expected = new List<byte>
			{
				expectedHeader, //Packet type, dup flag, retain
				(byte)(topicName.Length + 2 /*topic length*/ + 2 /*packet ID*/ + payload.Length), //Length
				0x00, (byte)topicName.Length
			};
			expected.AddRange(System.Text.Encoding.UTF8.GetBytes(topicName));
			expected.Add((byte)(packetId >> 8));
			expected.Add((byte)packetId);
			expected.AddRange(payload);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected.ToArray());
		}

		[Fact(DisplayName = "Serializes PubAck packet")]
		public void Serializes_Puback_Packet()
		{
			ushort packetId = 0x8178;
			var packet = new PubAck(packetId);
			byte[] expected =
			{
				0b01000000, //Type code
			    2, //Length
			    0x81, 0x78 //Packet ID
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		[Fact(DisplayName = "Serializes PubRec packet")]
		public void Serializes_Pubrec_Packet()
		{
			ushort packetId = 0x8179;
			var packet = new PubRec(packetId);
			byte[] expected =
			{
				0b01010000, //Type code
			    2, //Length
			    0x81, 0x79 //Packet ID
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		[Fact(DisplayName = "Serializes PubRel packet")]
		public void Serializes_Pubrel_Packet()
		{
			ushort packetId = 0x8079;
			var packet = new PubRel(packetId);
			byte[] expected =
			{
				0b01100010, //Type code
			    2, //Length
			    0x80, 0x79 //Packet ID
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		[Fact(DisplayName = "Serialzies PubComp packet")]
		public void Serializes_Pubcomp_Packet()
		{
			ushort packetId = 0x8989;
			var packet = new PubComp(packetId);
			byte[] expected =
			{
				0b01110000, //Type code
			    2, //Length
			    0x89, 0x89 //Packet ID
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		[Fact(DisplayName = "Serializes Subscribe packet")]
		public void Serializes_Subscribe_Packet()
		{
			ushort packetId = 0x8098;
			var packet = new Subscribe(packetId, new SubscriptionRequest[] { new("a/b", QosLevel.Qos1), new("c/d", QosLevel.Qos2) });
			byte[] expected =
			{
				0b10000010, //Type code
			    14, //Length
			    0x80, 0x98, //Packet ID
			    0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b" Qos1
			    0, 3, 0x63, 0x2f, 0x64, 2 //Filter "c/d" Qos2
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
		}

		[Fact(DisplayName = "Serializes SubAck packet")]
	    public void Serializes_SubAck_Packet()
	    {
		    ushort packetId = 0x6790;
		    var packet = new SubAck(packetId, new[] { SubscriptionResult.SuccessQos0, SubscriptionResult.SuccessQos2, SubscriptionResult.Failure });
		    byte[] expected =
		    {
			    0b10010000, //Type code
			    5, //Length
			    0x67, 0x90, //Packet ID
			    0, 0x02, 0x80 //Response codes
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
	    }

		[Fact(DisplayName = "Serializes Unsubscribe packet")]
	    public void Serializes_Unsubscribe_Packet()
	    {
		    ushort packetId = 0x0c1d;
		    var packet = new Unsubscribe(packetId, new[] { "a/b", "c/d" });
		    byte[] expected =
		    {
			    0b10100010, //Type code
			    12, //Length
			    0x0c, 0x1d, //Packet ID
			    0, 3, 0x61, 0x2f, 0x62, //Filter "a/b"
			    0, 3, 0x63, 0x2f, 0x64	//Filter "c/d"
		    };

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(expected);
	    }

		[Fact(DisplayName = "Serializes UnsubAck packet")]
	    public void Serializes_UnsubAck_Packet()
	    {
		    ushort packetId = 0x0990;
			var packet = new UnsubAck(packetId);

			Encoding.Encode(bufferWriter, packet);

			VerifySerialization(0b10110000, 0b00000010, 0x09, 0x90);
	    }

		[Fact(DisplayName = "Serializes PingReq packet")]
	    public void Serializes_PingReq_Packet()
	    {
			Encoding.Encode(bufferWriter, new PingReq());

		    VerifySerialization(0b11000000, 0b00000000);
	    }

		[Fact(DisplayName = "Serialzies PingResp packet")]
	    public void Serializes_PingResp_Packet()
	    {
			Encoding.Encode(bufferWriter, new PingResp());

		    VerifySerialization(0b11010000, 0b00000000);
	    }

		[Fact(DisplayName = "Serialzies Disconnect packet")]
	    public void Serializes_Disconnect_Packet()
	    {
			Encoding.Encode(bufferWriter, new Disconnect());

		    VerifySerialization(0b11100000, 0b00000000);
	    }

		private void VerifySerialization(params byte[] expected)
	    {
			var actual = buffer.Reader.GetMemory().ToArray();
            Assert.Equal(expected, actual);
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
            var memory = bufferWriter.GetMemory(expected.Length);

            int bytesWritten = memory.WriteRemainingLength(value);
            bufferWriter.Advance(bytesWritten);

            Assert.Equal(expected.Length, bytesWritten);
            Assert.Equal(expected, buffer.Reader.GetMemory().ToArray());
        }

        [Fact(DisplayName = "WriteString correctly serializes string")]
		public void WriteString_Correctly_Serializes_String()
		{			
			string value = Guid.NewGuid().ToString();
			byte[] expected = new byte[] { 0, (byte)value.Length }.Concat(System.Text.Encoding.UTF8.GetBytes(value)).ToArray();
            
            bufferWriter.WriteString(value);
			bufferWriter.Advance(expected.Length);
            			
			Assert.Equal(expected, buffer.Reader.GetMemory().ToArray());
		}
	}
}
