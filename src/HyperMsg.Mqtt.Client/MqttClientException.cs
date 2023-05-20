namespace HyperMsg.Mqtt.Client;

public class MqttClientException : Exception
{
    public MqttClientException()
    {
    }

    public MqttClientException(string? message) : base(message)
    {
    }

    public MqttClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
