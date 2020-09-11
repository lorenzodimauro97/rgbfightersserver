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
        //SetupConsumer();
        
    }

    public static async Task SingleProducerSingleConsumer()
    {
        var channel = _server.Channel;
        
        channel.r
    }
}
