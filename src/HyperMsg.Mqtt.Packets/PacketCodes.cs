namespace HyperMsg.Mqtt.Packets;

public static class PacketCodes
{
    public const byte Connect = 0b00010000;
    public const byte ConAck = 0b00100000;
    public const byte Unsubscribe = 0b10100010;
    public const byte SubAck = 0b10010000;
    public const byte Subscribe = 0b10000010;
    public const byte Pubcomp = 0b01110000;
    public const byte Pubrel = 0b01100010;
    public const byte Pubrec = 0b01010000;
    public const byte Puback = 0b01000000;
    public const byte UnsubAck = 0b10110000;
    public const byte PingReq = 0b11000000;
    public const byte PingResp = 0b11010000;
    public const byte Disconnect = 0b11100000;
}
