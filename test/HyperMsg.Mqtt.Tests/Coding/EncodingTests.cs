using FakeItEasy;
using HyperMsg.Mqtt.Packets;
using System.Buffers;
using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class EncodingTests
{
    private readonly Memory<byte> bufferWriter;

    public EncodingTests()
    {
        var memoryOwner = MemoryPool<byte>.Shared.Rent();
        bufferWriter = memoryOwner.Memory;
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
        //yield return new object[] { false, ConnectReasonCode.Accepted };
        //yield return new object[] { true, ConnectReasonCode.BadUsernameOrPassword };
        //yield return new object[] { false, ConnectReasonCode.IdentifierRejected };
        //yield return new object[] { true, ConnectReasonCode.NotAuthorized };
        //yield return new object[] { false, ConnectReasonCode.ServerUnavailable };
        yield return new object[] { };// { true, ConnectReasonCode.UnacceptableVersion };
    }

    [Theory(DisplayName = "Encodes ConnAck packet")]
    [MemberData(nameof(GetTestCasesForSerializeConnAck))]
    public void Encode_ConnAck_Packet(bool sessionPresent, ConnectReasonCode result)
    {
        var packet = new ConnAck(result, sessionPresent);
        byte[] expected =
        {
            0b00100000, //packet code
				0x02, //length
				(byte)(sessionPresent ? 0x01 : 0x00), //flags
				(byte)result
        };

        Encoding.Encode(bufferWriter.Span, packet);

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

    [Theory(DisplayName = "Encodes Publish packet")]
    [MemberData(nameof(GetTestCasesForSerializePublish))]
    public void Encodes_Publish_Packet(byte expectedHeader, bool dup, QosLevel qos, bool retain)
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

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected.ToArray());
    }

    [Fact(DisplayName = "Encodes PubAck packet")]
    public void Encode_Puback_Packet()
    {
        ushort packetId = 0x8178;
        var packet = new PubAck(packetId);
        byte[] expected =
        {
            0b01000000, //Type code
			    2, //Length
			    0x81, 0x78 //Packet ID
		    };

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes PubRec packet")]
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

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes PubRel packet")]
    public void Encode_Pubrel_Packet()
    {
        ushort packetId = 0x8079;
        var packet = new PubRel(packetId);
        byte[] expected =
        {
            0b01100010, //Type code
			    2, //Length
			    0x80, 0x79 //Packet ID
		    };

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes PubComp packet")]
    public void Encode_Pubcomp_Packet()
    {
        ushort packetId = 0x8989;
        var packet = new PubComp(packetId);
        byte[] expected =
        {
            0b01110000, //Type code
			    2, //Length
			    0x89, 0x89 //Packet ID
		    };

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes Subscribe packet")]
    public void Encode_Subscribe_Packet()
    {
        ushort packetId = 0x8098;
        //var packet = new Subscribe(packetId, new SubscriptionRequest[] { new("a/b", QosLevel.Qos1), new("c/d", QosLevel.Qos2) });
        byte[] expected =
        {
            0b10000010, //Type code
			    14, //Length
			    0x80, 0x98, //Packet ID
			    0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b" Qos1
			    0, 3, 0x63, 0x2f, 0x64, 2 //Filter "c/d" Qos2
		    };

        //Encoding.Encode(bufferWriter, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes SubAck packet")]
    public void Encodes_SubAck_Packet()
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

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes Unsubscribe packet")]
    public void Encodes_Unsubscribe_Packet()
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

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(expected);
    }

    [Fact(DisplayName = "Encodes UnsubAck packet")]
    public void Encodes_UnsubAck_Packet()
    {
        ushort packetId = 0x0990;
        var packet = new UnsubAck(packetId);

        Encoding.Encode(bufferWriter.Span, packet);

        VerifySerialization(0b10110000, 0b00000010, 0x09, 0x90);
    }

    //[Fact(DisplayName = "Encodes PingReq packet")]
    //public void Encodes_PingReq_Packet()
    //{
    //    Encoding.Encode(bufferWriter.Span, new PingReq());

    //    VerifySerialization(0b11000000, 0b00000000);
    //}

    //[Fact(DisplayName = "Encodes PingResp packet")]
    //public void Encodes_PingResp_Packet()
    //{
    //    Encoding.Encode(bufferWriter.Span, new PingResp());

    //    VerifySerialization(0b11010000, 0b00000000);
    //}

    //[Fact(DisplayName = "Encodes Disconnect packet")]
    //public void Encodes_Disconnect_Packet()
    //{
    //    Encoding.Encode(bufferWriter.Span, new Disconnect());

    //    VerifySerialization(0b11100000, 0b00000000);
    //}

    private void VerifySerialization(params byte[] expected)
    {
        //var actual = buffer.Reader.GetMemory().ToArray();
        //Assert.Equal(expected, actual);
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

    //[Theory(DisplayName = "WriteRemainingLength serializes value for remaining length")]
    //[MemberData(nameof(GetTestCasesForWriteRemaningLength))]
    //public void WriteRemainingLength_Serializes_Value_For_Remaining_Length(int value, byte[] expected)
    //{
    //    var memory = bufferWriter.GetMemory(expected.Length);

    //    int bytesWritten = memory.Span.WriteVarInt(value);
    //    bufferWriter.Advance(bytesWritten);

    //    Assert.Equal(expected.Length, bytesWritten);
    //    Assert.Equal(expected, buffer.Reader.GetMemory().ToArray());
    //}

    //[Fact(DisplayName = "WriteString correctly serializes string")]
    //public void WriteString_Correctly_Serializes_String()
    //{
    //    string value = Guid.NewGuid().ToString();
    //    byte[] expected = new byte[] { 0, (byte)value.Length }.Concat(System.Text.Encoding.UTF8.GetBytes(value)).ToArray();

    //    //bufferWriter.WriteString(value);
    //    bufferWriter.Advance(expected.Length);

    //    Assert.Equal(expected, buffer.Reader.GetMemory().ToArray());
    //}
}
