using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace HyperMsg.Mqtt.Serialization
{
    public static class PipeReaderExtensions
    {
	    public static async Task<Packet> ReadMqttPacketAsync(this PipeReader reader, CancellationToken token = default)
	    {
		    var result = await reader.ReadAsync(token);

			

		    return null;
	    }
    }
}
