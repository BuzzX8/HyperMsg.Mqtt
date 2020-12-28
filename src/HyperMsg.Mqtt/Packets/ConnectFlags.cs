using System;

namespace HyperMsg.Mqtt.Packets
{
    [Flags]
    public enum ConnectFlags : byte
    {
        None = 0,
        CleanSession = 0x02,
        Will = 0x04,
        Qos0 = 0x00,
        Qos1 = 0x08,
        Qos2 = 0x10,
        WillRetain = 0x20,
        Password = 0x40,
        UserName = 0x80
    }
}
