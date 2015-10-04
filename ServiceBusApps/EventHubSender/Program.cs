using System.Net.Http;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubSender
{
  class Program
  {


    static string _sasToken;
    static string _serviceNamespace = "nielsbevthub-ns";
    static string  _hubName = "nielsbevthub";
    static string _pubName = "somePublisherName";
    static string _senderKey = "OGfHD26gsRsQmjCdLc8M4vXHAtJ9V1MTaYho7Dm2oC4=";
    static string _senderKeyName = "publishRule";

    private static EventHubPublisher _eHub;

    static string _ehUrl = "sb://nielsbevthub-ns.servicebus.windows.net";

    static TimeSpan _ts = TimeSpan.FromHours(1);

    static void Main(string[] args)
    {

      Console.WriteLine("Press Ctrl-C to stop the sender process");
      Console.WriteLine("Press Enter to connect to EventHub");
      Console.ReadLine();


      _eHub = new EventHubPublisher(_ehUrl, _hubName, _senderKeyName, _senderKey);
      _eHub.Connect();

      Console.WriteLine("Connected. Press Enter to send msgs");
      Console.ReadLine();

      Send(20);

      var message = "{\"Name\":\"Hello World\", \"MsgNo\": 21}";
      SendMsg(message);



      //_sasToken = CreateForHttpSender(_senderKeyName, _senderKey, _serviceNamespace, _hubName, _pubName, _ts);

      ////SasTokenSample();

      //SendMessageHttps().Wait();

      ////SendingRandomMessages().Wait();

      Console.WriteLine("If we hit this, then something is wrong");
      Console.ReadLine();

    }

    static async Task Send(int nbrMsgs)
    {

      int nCount = 0;
      while (nCount < nbrMsgs)
      {
        nCount += 1;
        var message = "{\"Name\":\"Hello World\", \"MsgNo\":" + nCount.ToString() + "}";
        var msg = Encoding.UTF8.GetBytes(message);

       await _eHub.PostMsg(msg, partitionKey: "wager").ConfigureAwait(false);

      }


    }

    static async Task SendMsg(string msg)
    {
      //await EventHubRESTSender.Instance.SendMsg(msg);
    }

    static async Task SendMessageHttps()
    {
      int nCount = 0;
     
      var url = string.Format("{0}/publishers/{1}/messages", _hubName, _pubName);
      
      var httpClient = new HttpClient
      {
        BaseAddress = new Uri(string.Format("https://{0}.servicebus.windows.net/", _serviceNamespace))
      };

      httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _sasToken);
      while (nCount < 8)
      {
        
        nCount += 1;
        var message = "{\"Name\":\"Hello World\", \"MsgNo\":" + nCount.ToString() +"}";

        var content = new StringContent(message, Encoding.UTF8, "application/json");

        content.Headers.Add("ContentType", "application/atom+xml;type=entry;charset=utf-8");

        var resp = httpClient.PostAsync(url, content).Result;

        Console.WriteLine(" > Response: {0}",
          resp.StatusCode);
      }



    }

    public static void SasTokenSample()
    {

      Uri runtimeUri = ServiceBusEnvironment.CreateServiceUri("sb", "yournamespace", string.Empty);
      MessagingFactory mf = MessagingFactory.Create(runtimeUri,
          TokenProvider.CreateSharedAccessSignatureTokenProvider("device_send_listen",
          "AsjoWKeRn5fiyELz67vRa9Tk8TJo0sErXlL6jg7P6dFA="));

      //QueueClient sendClient = mf.CreateQueueClient(m_qName);

      ////Sending message to queue. 
      //BrokeredMessage helloMessage = new BrokeredMessage("Hello, Service Bus!");
      //helloMessage.MessageId = "Hello SAS token Message";
      //sendClient.Send(helloMessage);
    }


    public static string CreateForHttpSender(string senderKeyName, string senderKey, string serviceNamespace, string hubName, string publisherName, TimeSpan tokenTimeToLive)
    {
      var serviceUri = ServiceBusEnvironment.CreateServiceUri("https", serviceNamespace, String.Format("{0}/publishers/{1}/messages", hubName, publisherName))
          .ToString()
          .Trim('/');
      return SharedAccessSignatureTokenProvider.GetSharedAccessSignature(senderKeyName, senderKey, serviceUri, tokenTimeToLive);
    }

    static async Task SendingRandomMessages()
    {
      int nCount = 0;
      var eventHubName = "nielsbevthub";
      var eventHubConnectionString = GetEventHubConnectionString();
      var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);
      while (nCount < 8)
      {
        try
        {
          nCount += 1;
          var message = string.Format("{0}", nCount);
          Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), message);
          var msg = new EventData(Encoding.UTF8.GetBytes(message));
          
          //msg.PartitionKey = "Hello";
          //await eventHubClient.SendAsync(msg);
          eventHubClient.Send(msg);
        }
        catch (AggregateException agexp)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(agexp.Flatten());
          Console.ResetColor();
        }

        catch (Exception exception)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
          Console.ResetColor();
        }

        // we send it slowly...
        await Task.Delay(200);
      }
    }


    static string GetEventHubConnectionString()
    {
      var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
        return string.Empty;
      }
      try
      {
        var builder = new ServiceBusConnectionStringBuilder(connectionString);
        builder.TransportType = TransportType.Amqp;
        return builder.ToString();
      }
      catch (Exception exception)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.Message);
        Console.ResetColor();
      }

      return null;
    }

  }
}
