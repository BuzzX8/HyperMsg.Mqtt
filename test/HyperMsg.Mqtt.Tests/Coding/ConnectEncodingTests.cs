using HyperMsg.Mqtt.Packets;
using Xunit;

namespace HyperMsg.Mqtt.Coding;

public class ConnectEncodingTests
{
    private readonly byte[] buffer = new byte[1000];

    [Fact(DisplayName = "Encodes Connect packet with Password flag")]
    public void Encode_Connect_With_Password_Flag()
    {
        byte[] password = Guid.NewGuid().ToByteArray();
        var packet = Connect.NewV5(Guid.NewGuid().ToString()) with
        {
            Flags = ConnectFlags.Password,
            Password = password
        };

        var expected = CreateConnectHeader(packet);
        AddBinary(expected, password);
        SetRemainingLength(expected);

        Encoding.Encode(buffer, packet);

        VerifySerialization(expected.ToArray());
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
        var expected = CreateConnectHeader(packet);
        AddString(expected, username);
        SetRemainingLength(expected);

        Encoding.Encode(buffer, packet);

        VerifySerialization(expected.ToArray());
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
        var expected = CreateConnectHeader(packet);
        AddString(expected, willTopic);
        AddBinary(expected, willMessage);
        SetRemainingLength(expected);

        Encoding.Encode(buffer, packet);

        VerifySerialization(expected.ToArray());
    }

    public static IEnumerable<object[]> GetTestCasesForConnectEncoding()
    {
        yield return new object[] { ConnectFlags.None };
        yield return new object[] { ConnectFlags.CleanSession };
        yield return new object[] { ConnectFlags.WillRetain };
        yield return new object[] { ConnectFlags.Qos0 };
        yield return new object[] { ConnectFlags.Qos1 };
        yield return new object[] { ConnectFlags.Qos2 };
        yield return new object[] { ConnectFlags.CleanSession | ConnectFlags.WillRetain };
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
        var expected = CreateConnectHeader(packet);
        SetRemainingLength(expected);

        Encoding.Encode(buffer, packet);

        VerifySerialization(expected.ToArray());
    }

    private static List<byte> CreateConnectHeader(Connect packet)
    {
        List<byte> expected = new List<byte>
        {
            0x10, //Type code
				0, //Length placeholder
				0, 4, (byte)'M', (byte)'Q', (byte)'T', (byte)'T', //Protocol name
				5, //Protocol version
				(byte)packet.Flags, //Flags
				(byte)(packet.KeepAlive >> 8), (byte)packet.KeepAlive,
				//0, (byte)packet.ClientId.Length//Client ID length
			};
        AddString(expected, packet.ClientId);
        return expected;
    }

    private static void AddBinary(List<byte> packet, byte[] bytes)
    {
        packet.AddRange(new byte[] { 0, (byte)bytes.Length });
        packet.AddRange(bytes);
    }

    private static void AddString(List<byte> packet, string str)
    {
        packet.AddRange(new byte[] { 0, (byte)str.Length });
        packet.AddRange(System.Text.Encoding.UTF8.GetBytes(str));
    }

    private static void SetRemainingLength(List<byte> packet)
    {
        packet[1] = (byte)(packet.Count - 2);
    }

    private void VerifySerialization(params byte[] expected)
    {
        var actual = buffer[..(expected.Length)];
        Assert.Equal(expected, actual);
    }
}
