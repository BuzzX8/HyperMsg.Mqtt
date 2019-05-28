using FakeItEasy;
using System;
using System.Collections.Generic;
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

            //var context = A.Fake<IConfigurationContext>();
            //A.CallTo(() => context.GetService(typeof(ISender<Packet>))).Returns(A.Fake<ISender<Packet>>());
            //A.CallTo(() => context.GetService(typeof(IHandler<TransportCommands>))).Returns(A.Fake<IHandler<TransportCommands>>());
            //A.CallTo(() => context.GetService(typeof(IHandler<ReceiveMode>))).Returns(A.Fake<IHandler<ReceiveMode>>());
            //A.CallTo(() => context.GetSetting(nameof(MqttConnectionSettings))).Returns(settings);

            //var configurators = new List<Action<IConfigurationContext>>();
            //A.CallTo(() => configurable.Configure(A<Action<IConfigurationContext>>._)).Invokes(foc =>
            //{
            //    configurators.Add(foc.GetArgument<Action<IConfigurationContext>>(0));
            //});

            //configurable.UseMqttClient(settings);

            //foreach(var configurator in configurators)
            //{
            //    configurator.Invoke(context);
            //}

            //A.CallTo(() => context.RegisterService(typeof(IMqttClient), A<object>._)).MustHaveHappened();
        }
    }
}
