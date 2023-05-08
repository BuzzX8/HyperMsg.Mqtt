using Xunit;

namespace HyperMsg.Mqtt.Coding
{
    public class ConnectDecodingTests
    {
        private static readonly byte[] EncodedConnectPacket =
        {
            16, 171, 1, //Fixed header
            0, 4, 77, 81, 84, 84, //Protocol name (MQTT)
            5, //Protocol version
            198, //Flags
            0, 60, //Keep alive
            //Properties
            48, 17, 0, 0, 0, 11, 33, 117, 48, 39, 0, 0, 136, 184, 34, 0, 12, 25, 1, 23, 1, 38, 0, 5, 80, 114, 111, 112, 49, 0, 4, 86, 97, 108, 49, 38, 0, 5, 80, 114, 111, 112, 50, 0, 4, 86, 97, 108, 50, 0, 14, 109, 113, 116, 116, 120, 95, 52, 98, 57, 52, 50, 54, 55, 102, 51, 24, 0, 0, 0, 45, 1, 0, 2, 0, 0, 0, 26, 3, 0, 9, 115, 111, 109, 101, 45, 116, 121, 112, 101, 8, 0, 14, 114, 101, 115, 112, 111, 110, 115, 101, 45, 116, 111, 112, 105, 99, 9, 0, 7, 53, 50, 51, 53, 52, 51, 52, 0, 15, 108, 97, 115, 116, 45, 119, 105, 108, 108, 45, 116, 111, 112, 105, 99, 0, 0, 0, 8, 74, 111, 104, 110, 32, 68, 111, 101, 0, 13, 115, 111, 109, 101, 45, 112, 97, 115, 115, 119, 111, 114, 100
        };

        [Fact]
        public void DecodeConnect_Correctly_Decodes_Variable_Header()
        {
            var packet = Decoding.DecodeConnect(EncodedConnectPacket);

            Assert.Equal("MQTT", packet.ProtocolName);
            Assert.Equal(5, packet.ProtocolVersion);
            Assert.Equal(60, packet.KeepAlive);

            Assert.Equal(11u, packet.Properties.SessionExpiryInterval);
            Assert.Equal(30000u, packet.Properties.ReceiveMaximum);
            Assert.Equal(35000u, packet.Properties.MaximumPacketSize);
            Assert.Equal(12u, packet.Properties.TopicAliasMaximum);
            Assert.True(packet.Properties.RequestResponseInformation);
            Assert.True(packet.Properties.RequestProblemInformation);
            Assert.Equal("Val1", packet.Properties.UserProperties["Prop1"]);
            Assert.Equal("Val2", packet.Properties.UserProperties["Prop2"]);
        }
    }
}
