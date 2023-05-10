using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Coding
{
    public class DecodingTests
    {
        public static IEnumerable<object[]> DecodingTestCases()
        {
            yield return TestCase(new Disconnect(), 0b11100000, 0);
            yield return TestCase(new PingResp(), 0b11010000, 0);
            yield return TestCase(new PingReq(), 0b11000000, 0);
            yield return TestCase(new UnsubAck(8), 0b10110000, 2, 0, 8);
            yield return TestCase(new Unsubscribe(8, new[] { "a/b", "c/d" }),
                0b10100010, //Packet code
                12, //Length
                0, 8, //Packet Id
                0, 3, 0x61, 0x2f, 0x62, //Filter "a/b"
                0, 3, 0x63, 0x2f, 0x64 //Filter "c/d"
            );
            yield return TestCase(new SubAck(10, new[] { SubscriptionResult.SuccessQos0, SubscriptionResult.SuccessQos2, SubscriptionResult.Failure }),
                0b10010000, //Packet code
                5, //Length
                0, 10, //Packet Id
                0, 2, 0x80 //Response codes
            );
            //yield return TestCase(new Subscribe(4, new SubscriptionRequest[] { new("a/b", QosLevel.Qos1), new("c/d", QosLevel.Qos2) }),
            //    0b10000010, //Packet code
            //    14, //Length
            //    0, 4, //Packet ID
            //    0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b", Qos1
            //    0, 3, 0x63, 0x2f, 0x64, 2); //Filter "c/d", Qos2
            yield return TestCase(new PubComp(50), 0b01110000, 2, 0, 50);
            yield return TestCase(new PubRel(45), 0b01100010, 2, 0, 45);
            yield return TestCase(new PubRec(40), 0b01010000, 2, 0, 40);
            yield return TestCase(new PubAck(35), 0b01000000, 2, 0, 35);
            yield return TestCase(new Publish(10, "a/b", new byte[] { 9, 8, 7 }, QosLevel.Qos2)
            {
                Dup = true,
                Retain = true
            },
                0b00111101, //Packet code
                10, //Length
                0, 3, 0x61, 0x2f, 0x62, //Topic name
                0, 10, //Packet ID
                9, 8, 7 //Payload
            );
            yield return TestCase(new ConnAck(ConnectReasonCode.NotAuthorized, true),
                0b00100000, //Packet type
                2, //Packet length
                1, //Flags
                0x05); //Code of response				
        }

        private static object[] TestCase(object expected, params byte[] serialized) => new object[] { serialized, (serialized.Length, expected) };

        [Theory(DisplayName = "Decode return correct packet")]
        [MemberData(nameof(DecodingTestCases))]
        public void Decode_Returns_Correct_Packet(byte[] serialized)
        {
            var packet = Decoding.Decode(serialized, out var bytesConsumed);

            //Assert.Equal(expected, (bytesConsumed, packet));
        }

        public static IEnumerable<object[]> GetTestCasesForReadRemainingLength()
        {
            yield return GetTestCaseForReadRemainingLength(0, 0);
            yield return GetTestCaseForReadRemainingLength(127, 0x7f);
            yield return GetTestCaseForReadRemainingLength(128, 0x80, 0x01);
            yield return GetTestCaseForReadRemainingLength(16383, 0xff, 0x7f);
            yield return GetTestCaseForReadRemainingLength(16384, 0x80, 0x80, 0x01);
            yield return GetTestCaseForReadRemainingLength(2097151, 0xff, 0xff, 0x7f);
            yield return GetTestCaseForReadRemainingLength(2097152, 0x80, 0x80, 0x80, 0x01);
            yield return GetTestCaseForReadRemainingLength(268435455, 0xff, 0xff, 0xff, 0x7f);
        }

        private static object[] GetTestCaseForReadRemainingLength(int expected, params byte[] serialized)
        {
            return new object[] { serialized, expected };
        }

        [Theory(DisplayName = "ReadRemainingLength reads correct value")]
        [MemberData(nameof(GetTestCasesForReadRemainingLength))]
        public void ReadRemainingLength_Reads_Correct_Value(byte[] serialized)
        {
            var buffer = new ReadOnlySpan<byte>(serialized);

            (var length, var byteCount) = buffer.ReadVarInt();

            //Assert.Equal(expected, length);
            Assert.Equal(serialized.Length, byteCount);
        }

        [Fact]
        public void ReadRemainingLength_Throws_Exception()
        {
            var data = new byte[] { 0xff, 0xff, 0xff, 0x80, 0x08 };
            var buffer = new ReadOnlyMemory<byte>(data);

            Assert.Throws<FormatException>(() => buffer.Span.ReadVarInt());
        }


        [Fact(DisplayName = "ReadString correctly reads string")]
        public void ReadString_Correctly_Reads_String()
        {
            string expected = Guid.NewGuid().ToString();
            var bytes = new List<byte> { 0, (byte)expected.Length };
            bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(expected));
            var buffer = new ReadOnlySpan<byte>(bytes.ToArray());

            string actual = buffer.ReadString();

            Assert.Equal(expected, actual);
        }
    }
}
