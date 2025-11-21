namespace HyperMsg.Mqtt.Packets
{
    public class PublishProperties
    {
        public byte PayloadFormatIndicator { get; set; }
        public uint MessageExpiryInterval { get; set; }
        public ushort TopicAlias { get; set; }
        public string ResponseTopic { get; set; }
        public ReadOnlyMemory<byte> CorrelationData { get; set; }
        public Dictionary<string, string> UserProperties { get; set; }
        public int SubscriptionIdentifier { get; set; }
        public string ContentType { get; set; }
    }
}
