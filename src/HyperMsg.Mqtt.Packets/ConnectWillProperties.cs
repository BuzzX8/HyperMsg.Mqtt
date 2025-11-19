namespace HyperMsg.Mqtt.Packets
{
    public class ConnectWillProperties
    {
        public byte PayloadFormatIndicator { get; set; }

        public uint MessageExpiryInterval { get; set; }

        public uint WillDelayInterval { get; set; }
        public string ContentType { get; internal set; }

        public string ResponseTopic { get; internal set; }

        public ReadOnlyMemory<byte> CorrelationData { get; internal set; }

        public IDictionary<string, string> UserProperties { get; internal set; }
    }
}
