using HyperMsg.Messaging;

namespace HyperMsg.Mqtt.Coding;

public class DecodingComponent : IMessagingComponent
{
    private readonly IMessagingContext _messagingContext;

    public void Attach(IMessagingContext messagingContext)
    {
        throw new NotImplementedException();
    }

    public void Detach(IMessagingContext messagingContext)
    {
        throw new NotImplementedException();
    }
}
