using FakeItEasy;
using System;
using Xunit;

namespace HyperMsg.Mqtt.Client
{
    public class ConfigurableExtensionTests
    {
        [Fact]
        public void UseMqttClient_Adds_Configurator_Which_Registers_IMqttClient_Service()
        {
            var settings = new MqttConnectionSettings(Guid.NewGuid().ToString());
            var configurable = A.Fake<IConfigurable>();

            configurable.UseMqttClient(settings);

            A.CallTo(() => configurable.RegisterService(typeof(IMqttClient), A<ServiceFactory>._)).MustHaveHappened();
        }
    }
}
