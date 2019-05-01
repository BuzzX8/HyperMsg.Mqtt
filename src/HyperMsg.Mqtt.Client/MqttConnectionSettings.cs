using System;
using System.Security;

namespace HyperMsg.Mqtt.Client
{
    public class MqttConnectionSettings
    {
        public MqttConnectionSettings(string clientId)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        }

        public string ClientId { get; }

        public string UserName { get; }

        public SecureString Password { get; }
    }
}