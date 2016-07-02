using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MGS.Base.EventHub;

namespace EventCollEmulator
{
  internal class EventHubSender
  {
    readonly string _hubName = "nielsbevthub";
    readonly string _senderKey = "OGfHD26gsRsQmjCdLc8M4vXHAtJ9V1MTaYho7Dm2oC4=";
    readonly string _senderKeyName = "publishRule";
    readonly string _ehUrl = "sb://nielsbevthub-ns.servicebus.windows.net";

    private readonly EventHubPublisher _ehPublisher;

   public EventHubSender()
    {

      try
      {
        Console.WriteLine("Setting up EventBub sender aginst: {0}, EventHub: {1}", _ehUrl, _hubName);
        _ehPublisher = new EventHubPublisher(_ehUrl, _hubName, _senderKeyName, _senderKey);
        _ehPublisher.Connect();

       
      }

      catch (Exception e)
      {
        Console.WriteLine(e.Message);

      }

    }

    public async Task Send(byte[] msg, string partitionKey = null, IDictionary<string, object> sysProperties = null, IDictionary<string, object> properties = null)
    {

      var ret = await _ehPublisher.PostMsg(msg, partitionKey, sysProperties, properties);

    }

    public void ShutDown()
    {
      _ehPublisher.Shutdown();

    }



  }
}
