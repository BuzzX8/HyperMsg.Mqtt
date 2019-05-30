using System;

namespace HyperMsg.Mqtt.Client
{
    public class MqttConnectionSettings
    {
        public MqttConnectionSettings(string clientId)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        }

        /// <summary>
        /// Unique ID of client for server.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Time interval measured in seconds.
        /// </summary>
        public ushort KeepAlive { get; set; }

        public WillMessageSettings WillMessageSettings { get; set; }

        public bool UseTls { get; set; }
    }
}