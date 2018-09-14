using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using Xunit;

namespace HyperMsg.Mqtt.Serialization.Tests
{
    public class PipeReaderExtensionTests
    {
		public static IEnumerable<object[]> GetTestCasesForDeserialize()
		{
			yield return GetTestCaseForDeserialize(new Disconnect(), 0b11100000, 0);
			yield return GetTestCaseForDeserialize(new PingResp(), 0b11010000, 0);
			yield return GetTestCaseForDeserialize(new PingReq(), 0b11000000, 0);
			yield return GetTestCaseForDeserialize(new UnsubAck(8), 0b10110000, 2, 0, 8);
			yield return GetTestCaseForDeserialize(new Unsubscribe(8, "a/b", "c/d"),
				0b10100010, //Packet code
				12, //Length
				0, 8, //Packet Id
				0, 3, 0x61, 0x2f, 0x62, //Filter "a/b"
				0, 3, 0x63, 0x2f, 0x64 //Filter "c/d"
			);
			yield return GetTestCaseForDeserialize(new SubAck(10, SubscriptionResult.SuccessQos0, SubscriptionResult.SuccessQos2, SubscriptionResult.Failure),
				0b10010000, //Packet code
				5, //Length
				0, 10, //Packet Id
				0, 2, 0x80 //Response codes
			);
			yield return GetTestCaseForDeserialize(new Subscribe(4, ("a/b", QosLevel.Qos1), ("c/d", QosLevel.Qos2)),
				0b10000010, //Packet code
				14, //Length
				0, 4, //Packet ID
				0, 3, 0x61, 0x2f, 0x62, 1, //Filter "a/b", Qos1
				0, 3, 0x63, 0x2f, 0x64, 2); //Filter "c/d", Qos2
			yield return GetTestCaseForDeserialize(new PubComp(50), 0b01110000, 2, 0, 50);
			yield return GetTestCaseForDeserialize(new PubRel(45), 0b01100010, 2, 0, 45);
			yield return GetTestCaseForDeserialize(new PubRec(40), 0b01010000, 2, 0, 40);
			yield return GetTestCaseForDeserialize(new PubAck(35), 0b01000000, 2, 0, 35);
			yield return GetTestCaseForDeserialize(new Publish(10)
			{
				Dup = true,
				Qos = QosLevel.Qos2,
				Retain = true,
				Topic = "a/b",
				Message = new byte[] { 9, 8, 7 }
			},
				0b00111101, //Packet code
				10, //Length
				0, 3, 0x61, 0x2f, 0x62, //Topic name
				0, 10, //Packet ID
				9, 8, 7 //Payload
			);
			yield return GetTestCaseForDeserialize(new ConnAck(ConnectionResult.NotAuthorized, true),
				0b00100000, //Packet type
				2, //Packet length
				1, //Flags
				0x05); //Code of response				
		}

		private static object[] GetTestCaseForDeserialize(object expected, params byte[] serialized)
		{
			return new[] { serialized, expected };
		}

	    [Theory(DisplayName = "Deserialize correctly deserializes packets")]
	    [MemberData(nameof(GetTestCasesForDeserialize))]
		private void VerifyDeserialization(byte[] serialized, Packet expected)
	    {
		    var pipe = new Pipe();
		    pipe.Writer.WriteAsync(serialized, CancellationToken.None).AsTask().Wait();

		    var actual = pipe.Reader.ReadMqttPacketAsync(CancellationToken.None).Result;

			Assert.Equal(expected, actual);
	    }
    }
}
