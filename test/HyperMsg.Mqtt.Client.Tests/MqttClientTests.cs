using FakeItEasy;
using HyperMsg.Transport;
using Xunit;

namespace HyperMsg.Mqtt.Client;

public class MqttClientTests
{
    private readonly MqttClient _mqttClient;
    private readonly IClientContext _clientContext;
    private readonly ConnectionSettings _connectionSettings;

    public MqttClientTests()
    {
        _clientContext = A.Fake<IClientContext>();
        _connectionSettings = new ConnectionSettings(Guid.NewGuid().ToString());
        _mqttClient = new MqttClient(_clientContext, _connectionSettings);
    }

    [Fact]
    public async Task ConnectAsync_ShouldInvokeConnectionConnectAsync()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var connection = A.Fake<IConnection>();
        A.CallTo(() => _clientContext.Connection).Returns(connection);
        // Act
        await _mqttClient.ConnectAsync(cancellationToken);
        // Assert
        A.CallTo(() => connection.OpenAsync(cancellationToken)).MustHaveHappenedOnceExactly();
    }
}
