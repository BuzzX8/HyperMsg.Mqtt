namespace HyperMsg.Mqtt.Packets
{
    public class SubscribeProperties
    {
        public int SubscriptionIdentifier { get; set; }
        public Dictionary<string, string> UserProperties { get; set; }
    }
}