using System.Net;
using Microsoft.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace EventHubSender
{
  public class EventHubPublisher
  {
    private readonly string _eventHubName;
    private string _eventHubUrl;
    private string _connectionString;
    private readonly string _senderKeyName;
    private readonly string _senderKey;
    private readonly int _poolSize;

    private readonly ConcurrentStack<EventHubClient> _ehClients;
    private readonly ConcurrentStack<MessagingFactory> _msgFactories;

    
    public EventHubPublisher(string eventHubUrl, string eventHubName, string senderKeyName, string senderKey, int poolSize = 5)
    {
      _eventHubUrl = eventHubUrl;
      _eventHubName = eventHubName;
      _senderKeyName = senderKeyName;
      _senderKey = senderKey;
      _poolSize = poolSize;

      _ehClients = new ConcurrentStack<EventHubClient>();


    }

    public bool Connect()
    {
      try
      {

        CreateConnectionString();
        CreateEventHubPool();

        return true;
      }
      catch (Exception e)
      {
        var errMsg =
          $"Error when trying to connect to EventHub. Url: {_eventHubUrl}. EventHubName: {_eventHubName}. KeyName: {_senderKeyName}. Error: {e.Message}";
        throw new ApplicationException(errMsg);
      }

    }


    private void CreateEventHubPool()
    {

      for (int i = 0; i < _poolSize; i++)
      {
        var fact = MessagingFactory.CreateFromConnectionString(_connectionString);
        _msgFactories.Push(fact);
        var ehClient = fact.CreateEventHubClient(_eventHubName);
        _ehClients.Push(ehClient);
      }
    
   
    }


    void CreateConnectionString()
    {
      if (string.IsNullOrEmpty(_eventHubUrl))
      {
        throw new ApplicationException("Url for EventHub cannot be null or empty string");
      }

      if (_eventHubUrl.LastIndexOf("/", StringComparison.Ordinal) < 10)
      {
        _eventHubUrl = string.Concat(_eventHubUrl, "/");
      }

      _connectionString = $"Endpoint={_eventHubUrl};SharedAccessKeyName={_senderKeyName};SharedAccessKey={_senderKey}";

    }


    public async Task<bool> PostMsg(byte[] msg, string partitionKey = null, IDictionary<string, object> properties = null)
    {
      EventHubClient value = null;
      int channelRetrieveCounter = 0;
      try
      {

        var ed = new EventData(msg);
        if (!string.IsNullOrEmpty(partitionKey))
        {
          ed.PartitionKey = partitionKey;
        }

        if (properties != null && properties.Count > 0)
        {
          foreach (var kvp in properties)
          {
            ed.Properties.Add(kvp.Key, kvp.Value);
          }
        }

        while (!_ehClients.TryPop(out value))
        {
          Thread.Sleep(50);
          if (Interlocked.Increment(ref channelRetrieveCounter) > (_poolSize * 2))
          {
           return false;
          }
        }

        await value.SendAsync(ed);

        return true;
      }
      catch (AggregateException agexp)
      {
        
        var errMsg = $"Error when trying to send to EventHub. Url: {_eventHubUrl}. EventHubName: {_eventHubName}. Error: {agexp.Flatten().Message}";
        throw new ApplicationException(errMsg);
      }

      catch (Exception ex)
      {
        var errMsg = $"Error when trying to send to EventHub. Url: {_eventHubUrl}. EventHubName: {_eventHubName}. Error: {ex.Message}";
        throw new ApplicationException(errMsg);
      }

      finally
      {
        if (value != null)
        {
          _ehClients.Push(value);
        }
      }
    }


    public void Shutdown()
    {
     
      try
      {
        while (_ehClients.Count > 0)
        {
          EventHubClient eh;
          if (!_ehClients.TryPop(out eh)) continue;
          if (!eh.IsClosed)
          {
            eh.Close();
          }

          eh = null;
          
        }

        while (_msgFactories.Count > 0)
        {
          MessagingFactory mf;
          if (!_msgFactories.TryPop(out mf)) continue;
          if (!mf.IsClosed)
          {
            mf.Close();
          }
          mf = null;
        }

      }
      finally
      {
        _ehClients.Clear();
        _msgFactories.Clear();
      }



  }

  }
}
