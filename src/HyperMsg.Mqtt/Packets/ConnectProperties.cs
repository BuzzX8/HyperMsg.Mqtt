namespace HyperMsg.Mqtt.Packets
{
    public class ConnectProperties
    {
        public uint SessionExpiryInterval { get; set; }

        public ushort ReceiveMaximum { get; set; }

        public uint MaximumPacketSize { get; set; }

        public ushort TopicAliasMaximum { get; set; }

        public bool RequestResponseInformation { get; set; }

        public bool RequestProblemInformation { get; set; }

        public string AuthenticationMethod { get; set; }

        public byte[] AuthenticationData { get; set; }

        public IDictionary<string, string> UserProperties { get; set; }
    }
}
