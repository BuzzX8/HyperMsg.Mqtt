using FakeItEasy;
using System;
using Xunit;

namespace HyperMsg.Mqtt.Serialization
{
    public class ConfigurableExtensionTests
    {
        [Fact]
        public void UseMqttSerializer_Registers_Serializer_Service()
        {
            var configurable = A.Fake<IConfigurable>();            
            var context = A.Fake<IConfigurationContext>();
            var configurator = default(Action<IConfigurationContext>);
            A.CallTo(() => configurable.Configure(A<Action<IConfigurationContext>>._)).Invokes(foc =>
            {
                configurator = foc.GetArgument<Action<IConfigurationContext>>(0);
            });

            configurable.UseMqttSerializer();
            Assert.NotNull(configurator);
            configurator.Invoke(context);

            A.CallTo(() => context.RegisterService(typeof(ISerializer<Packet>), A<object>._)).MustHaveHappened();
        }
    }
}
