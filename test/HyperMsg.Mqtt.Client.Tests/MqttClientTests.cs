using System;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class MqttClientTests
    {
        private readonly MqttClient client;
        private readonly IConnection connection;
        private readonly ISender<Packet> sender;
    }
}