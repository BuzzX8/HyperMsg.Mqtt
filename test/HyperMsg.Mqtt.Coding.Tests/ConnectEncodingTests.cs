using HyperMsg.Mqtt.Packets;

namespace HyperMsg.Mqtt.Coding;

public class ConnectEncodingTests
{
    [Fact(DisplayName = "Encodes Connect packet with Password flag")]
    public void Encode_Connect_With_Password_Flag()
    {
        byte[] password = Guid.NewGuid().ToByteArray();
        var packet = Connect.NewV5(Guid.NewGuid().ToString()) with
        {
            Flags = ConnectFlags.Password,
            Password = password
        };

        VerifyEncoding(packet);
    }

    [Fact(DisplayName = "Encode Connect with Username flag")]
    public void Encode_Connect_With_Username_Flag()
    {
        string username = Guid.NewGuid().ToString();
        var packet = Connect.NewV5(Guid.NewGuid().ToString()) with
        {
            Flags = ConnectFlags.UserName,
            UserName = username
        };

        VerifyEncoding(packet);
    }

    [Fact(DisplayName = "Encodes Connect with Will flag")]
    public void Encode_Connect_With_Will_Flag()
    {
        string willTopic = Guid.NewGuid().ToString();
        byte[] willMessage = Guid.NewGuid().ToByteArray();
        var packet = Connect.NewV5(Guid.NewGuid().ToString()) with
        {
            Flags = ConnectFlags.Will,
            WillTopic = willTopic,
            WillPayload = willMessage
        };

        VerifyEncoding(packet);
    }

    public static TheoryData<ConnectFlags> GetTestCasesForConnectEncoding()
    {
        return
        [
            ConnectFlags.None,
            ConnectFlags.CleanSession,
            ConnectFlags.WillRetain,
            ConnectFlags.Qos0,
            ConnectFlags.Qos1,
            ConnectFlags.Qos2,
            ConnectFlags.CleanSession | ConnectFlags.WillRetain
        ];
    }

    [Theory(DisplayName = "Encodes Connect packet")]
    [MemberData(nameof(GetTestCasesForConnectEncoding))]
    public void Encode_Connect(ConnectFlags flags)
    {
        ushort keepAlive = BitConverter.ToUInt16(Guid.NewGuid().ToByteArray(), 0);
        string clientId = Guid.NewGuid().ToString();
        var packet = Connect.NewV5(clientId) with
        {
            Flags = flags,
            KeepAlive = keepAlive
        };

        VerifyEncoding(packet);
    }

    private static void VerifyEncoding(Connect packet)
    {
        var buffer = new byte[1000];

        Encoding.Encode(buffer, packet, out var bytesWritten);

        var decodedPacket = Decoding.Decode(buffer[..bytesWritten], out var bytesRead).ToConnect();

        Assert.Equal(bytesWritten, bytesRead);
        Assert.Equal(packet, decodedPacket);
    }
}
