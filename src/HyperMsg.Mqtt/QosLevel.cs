namespace HyperMsg.Mqtt
{
    public enum QosLevel : byte
    {
		None = byte.MaxValue,
		Qos0 = 0,
		Qos1,
		Qos2
    }
}
