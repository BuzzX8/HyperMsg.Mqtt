using HyperMsg.Mqtt.Packets;
using Microsoft.Extensions.DependencyInjection;
using static HyperMsg.Mqtt.Encoding;
using static HyperMsg.Mqtt.MqttDeserializer;

namespace HyperMsg.Mqtt;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMqttCoding(this IServiceCollection services)
    {            
        var compositeEncoder = new CompositeEncoder();
        AddEncoders(compositeEncoder);

        return services;
        //    .AddSerializer(compositeEncoder)
        //    .AddDeserializer(ReadBuffer);
    }

    private static void AddEncoders(CompositeEncoder encoder)
    {
        encoder.Add<Connect>(Encode);
        encoder.Add<ConnAck>(Serialize);
        encoder.Add<Subscribe>(Serialize);
        encoder.Add<SubAck>(Serialize);
        encoder.Add<Unsubscribe>(Serialize);
        encoder.Add<UnsubAck>(Serialize);
        encoder.Add<Publish>(Serialize);
        encoder.Add<PubAck>(Serialize);
        encoder.Add<PubRec>(Serialize);
        encoder.Add<PubRel>(Serialize);
        encoder.Add<PubComp>(Serialize);
        encoder.Add<PingReq>(Serialize);
        encoder.Add<PingResp>(Serialize);
        encoder.Add<Disconnect>(Serialize);
    }

    //private static void DeregisterSerializers(CompositeSerializer compositeSerializer)
    //{
    //    compositeSerializer.Deregister<Connect>();
    //    compositeSerializer.Deregister<ConnAck>();
    //    compositeSerializer.Deregister<Subscribe>();
    //    compositeSerializer.Deregister<SubAck>();
    //    compositeSerializer.Deregister<Unsubscribe>();
    //    compositeSerializer.Deregister<UnsubAck>();
    //    compositeSerializer.Deregister<Publish>();
    //    compositeSerializer.Deregister<PubAck>();
    //    compositeSerializer.Deregister<PubRec>();
    //    compositeSerializer.Deregister<PubRel>();
    //    compositeSerializer.Deregister<PubComp>();
    //    compositeSerializer.Deregister<PingReq>();
    //    compositeSerializer.Deregister<PingResp>();
    //    compositeSerializer.Deregister<Disconnect>();
    //}
}
