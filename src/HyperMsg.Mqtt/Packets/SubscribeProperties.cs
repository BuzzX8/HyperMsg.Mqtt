namespace HyperMsg.Mqtt.Packets
{
    public class SubscribeProperties
    {
        public int SubscriptionIdentifier { get; internal set; }
        public Dictionary<string, string> UserProperties { get; internal set; }
    }
}