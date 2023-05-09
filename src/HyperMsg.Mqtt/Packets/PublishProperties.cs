namespace HyperMsg.Mqtt.Packets
{
    public class PublishProperties
    {
        public byte PayloadFormatIndicator { get; internal set; }
        public uint MessageExpiryInterval { get; internal set; }
        public ushort TopicAlias { get; internal set; }
        public string ResponseTopic { get; internal set; }
        public ReadOnlyMemory<byte> CorrelationData { get; internal set; }
        public Dictionary<string, string> UserProperties { get; internal set; }
        public int SubscriptionIdentifier { get; internal set; }
        public string ContentType { get; internal set; }
    }
}
