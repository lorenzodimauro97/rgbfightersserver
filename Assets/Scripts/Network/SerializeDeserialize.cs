using System.Collections;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Network;
using UnityEngine;

public class SerializeDeserialize
{
    private static TrueServer _server;

    public SerializeDeserialize(TrueServer server)
    {
        _server = server;
        SetupConsumer();
        
    }

    public static async Task SingleProducerSingleConsumer()
    {
        var channel = _server.Channel;

        // In this example, the consumer keeps up with the producer
        
        var consumer1 = new Consumer(channel.Reader, 1, 1500);

        Task consumerTask1 = consumer1.ConsumeData(); // begin consuming

        await producerTask1.ContinueWith(_ => channel.Writer.Complete());

        await consumerTask1;
    }
}
