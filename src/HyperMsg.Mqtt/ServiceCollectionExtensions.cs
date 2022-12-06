using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.Encoding;

namespace HyperMsg.Mqtt;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMqttCoding(this IServiceCollection services)
    {            
        var compositeEncoder = new CompositeEncoder();
        AddEncoders(compositeEncoder);

        return services.AddCodingService(Decoding.Decode, compositeEncoder);
    }

    private static void AddEncoders(CompositeEncoder encoder)
    {
        encoder.Add<Connect>(Encode);
        encoder.Add<ConnAck>(Encode);
        encoder.Add<Subscribe>(Encode);
        encoder.Add<SubAck>(Encode);
        encoder.Add<Unsubscribe>(Encode);
        encoder.Add<UnsubAck>(Encode);
        encoder.Add<Publish>(Encode);
        encoder.Add<PubAck>(Encode);
        encoder.Add<PubRec>(Encode);
        encoder.Add<PubRel>(Encode);
        encoder.Add<PubComp>(Encode);
        encoder.Add<PingReq>(Encode);
        encoder.Add<PingResp>(Encode);
        encoder.Add<Disconnect>(Encode);
    }
}
