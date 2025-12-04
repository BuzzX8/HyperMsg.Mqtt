namespace HyperMsg.Mqtt.Coding;

internal class ConnectDecodingTestData
{
    // Minimal valid CONNECT packet (MQTT v5, empty Client ID)
    public static readonly byte[] CorrectPacketWithEmptyClientId =
    [
        0x10,       // Fixed header: CONNECT
        0x0B,       // Remaining length = 11
        0x00, 0x04, // Protocol Name length
        0x4D, 0x51, 0x54, 0x54, // "MQTT"
        0x05,       // Protocol Level = 5
        0x02,       // Connect Flags (Clean Start)
        0x00, 0x3C, // Keep Alive = 60
        0x00, 0x00  // Client ID length = 0 (empty)
    ];

    // Minimal valid CONNECT packet (MQTT v5, Client ID = "c")
    public static readonly byte[] CorrectPacketMinimal =
    [
        0x10,       // Fixed header: CONNECT
        0x0C,       // Remaining length = 12
        0x00, 0x04, // Protocol Name length
        0x4D, 0x51, 0x54, 0x54, // "MQTT"
        0x05,       // Protocol Level = 5
        0x02,       // Connect Flags (Clean Start)
        0x00, 0x3C, // Keep Alive = 60
        0x00, 0x01, // Client ID length = 1
        0x63        // "c"
    ];

    // CONNECT packet with various properties and payload fields set
    public static readonly byte[] CorrectPacketWithProperties =
    [
        16, 171, 1, //Fixed header
        0, 4, 77, 81, 84, 84, //Protocol name (MQTT)
        5, //Protocol version
        198, //Flags
        0, 60, //Keep alive
        //Properties
        48, 17, 0, 0, 0, 11, 33, 117, 48, 39, 0, 0, 136, 184, 34, 0, 12, 25, 1, 23, 1, 38, 0, 5, 80, 114, 111, 112, 49, 0, 4, 86, 97, 108, 49, 38, 0, 5, 80, 114, 111, 112, 50, 0, 4, 86, 97, 108, 50, 
        //Payload
        0, 14, 109, 113, 116, 116, 120, 95, 52, 98, 57, 52, 50, 54, 55, 102, //Client ID
        51, 24, 0, 0, 0, 45, 1, 0, 2, 0, 0, 0, 26, 3, 0, 9, 115, 111, 109, 101, 45, 116, 121, 112, 101, 8, 0, 14, 114, 101, 115, 112, 111, 110, 115, 101, 45, 116, 111, 112, 105, 99, 9, 0, 7, 53, 50, 51, 53, 52, 51, 52, 0, 15, 108, 97, 115, 116, 45, 119, 105, 108, 108, 45, 116, 111, 112, 105, 99, 0, 0, 0, 8, 74, 111, 104, 110, 32, 68, 111, 101, 0, 13, 115, 111, 109, 101, 45, 112, 97, 115, 115, 119, 111, 114, 100
    ];

    // CONNECT packet with incorrect protocol name ("MQTS" instead of "MQTT")
    public static readonly byte[] InvalidProtocolName =
    [
        0x10,       // Fixed header: CONNECT
        0x12,       // Remaining length = 18
        0x00, 0x04, // Protocol Name length
        0x4D, 0x51, 0x54, 0x53, // "MQTS" (invalid)
        0x05,       // Protocol Level = 5
        0x02,       // Connect Flags (Clean Start)
        0x00, 0x3C, // Keep Alive = 60
        0x00, 0x0A, // Client ID length = 10
        0x74, 0x65, 0x73, 0x74, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74 // "testClient"
    ];
}
