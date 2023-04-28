namespace HyperMsg.Mqtt.Coding
{
    public class DecodingError : Exception
    {
        public DecodingError(string message) : base(message)
        { }
    }
}
