using FakeItEasy;
using System;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class ConfigurableExtensionTests
    {
        [Fact]
        public void AddMqttClient_Adds_Configurator_Which_Registers_IMqttClient_Service()
        {
            var settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            var configurable = A.Fake<IServiceRegistry>();

            configurable.AddMqttClient(settings);

            A.CallTo(() => configurable.Add(typeof(IMqttClient), A<Func<IServiceProvider, object>>._)).MustHaveHappened();
        }
    }
}
