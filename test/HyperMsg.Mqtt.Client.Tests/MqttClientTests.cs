using FakeItEasy;

namespace HyperMsg.Mqtt.Client;

public class MqttClientTests
{
    private readonly MqttClient _mqttClient;
    private readonly IClientContext _clientContext;

    public MqttClientTests()
    {
        _clientContext = A.Fake<IClientContext>();
    }
}
