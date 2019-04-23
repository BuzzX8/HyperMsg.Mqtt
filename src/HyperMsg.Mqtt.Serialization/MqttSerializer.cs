using System.Buffers;

namespace HyperMsg.Mqtt.Serialization
{
    public class MqttSerializer : ISerializer<Packet>
    {
        public DeserializationResult<Packet> Deserialize(ReadOnlySequence<byte> buffer) => buffer.ReadMqttPacket();

        public void Serialize(IBufferWriter<byte> writer, Packet message) => writer.WriteMqttPacket(message);
    }
}
