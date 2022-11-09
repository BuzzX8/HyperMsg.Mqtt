namespace HyperMsg.Mqtt.Packets;

public enum ConnectionResult
{
    Accepted,
    UnacceptableVersion,
    IdentifierRejected,
    ServerUnavailable,
    BadUsernameOrPassword,
    NotAuthorized
}
