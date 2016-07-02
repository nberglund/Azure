using System.Net;
using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace EventHubSender
{
  public class EventHubPublisher2
  {
    private static object syncRoot = new Object();

    string _serviceNamespace = "nielsbevthub-ns";
    string _hubName = "nielsbevthub";
    string _pubName = "somePublisherName";
    string _senderKey = "OGfHD26gsRsQmjCdLc8M4vXHAtJ9V1MTaYho7Dm2oC4=";
    string _senderKeyName = "publishRule";
    static TimeSpan _ts = TimeSpan.FromHours(1);

    private HttpClient _httpClient;
    private string _serverUrl;
    private string _connectionString;
    private EventHubClient _ehClient;

    
    
    
   

    public EventHubPublisher2()
    {
      //_url = string.Format("{0}/publishers/{1}/messages", _hubName, _pubName);
      //InitialiseHttpClient();

    }

    public EventHubPublisher2(string serverUrl, string senderKeyName, string senderKey, string hubName)
    {
      _serverUrl = serverUrl;
      _senderKeyName = senderKeyName;
      _senderKey = senderKey;
      _hubName = hubName;
      var connString = CreateConnectionString();
      CreateHubClient(connString, _hubName);

    }

    void CreateHubClient(string connString, string hubName)
    {
      _ehClient = EventHubClient.CreateFromConnectionString(connString, hubName);
      
    }

    //Endpoint=sb://nielsbevthub-ns.servicebus.windows.net/;SharedAccessKeyName=publishRule;SharedAccessKey=OGfHD26gsRsQmjCdLc8M4vXHAtJ9V1MTaYho7Dm2oC4=

    string CreateConnectionString()
    {
      _connectionString =  string.Format("Endpoint={0};SharedAccesKeyName={1};SharedAccessKey={2}", string.Concat(_serverUrl, "/"),
        _senderKeyName, _senderKey);
      return _connectionString;
      
    }

    public async Task<bool> SendMsg(string message )
    {
      var content = new StringContent(message, Encoding.UTF8, "application/json");
      content.Headers.Add("ContentType", "application/atom+xml;type=entry;charset=utf-8");

      var resp = await _httpClient.PostAsync(_httpClient.BaseAddress + "/messages", content).ConfigureAwait(false);

      if (!resp.IsSuccessStatusCode)
      {
        Console.WriteLine("No success. Code: {0}", resp.StatusCode);
        return false;
      }
      else
      {
        return true;
      }

    }

    void InitialiseHttpClient()
    {
      ServicePointManager.DefaultConnectionLimit = 8;
      
      _httpClient = new HttpClient
      {
        BaseAddress = new Uri(string.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}", _serviceNamespace, _hubName, _pubName).ToLower())
      };

      GenerateAndSetSASToken();
      
    }

    void GenerateAndSetSASToken()
    {
      var sasToken = GenerateSASToken(_senderKeyName, _senderKey, _serviceNamespace, _hubName, _pubName, _ts);
      _httpClient.DefaultRequestHeaders.Clear();
      _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sasToken);
    }


    string GenerateSASToken(string senderKeyName, string senderKey, string serviceNamespace, string hubName, string publisherName, TimeSpan tokenTimeToLive)
    {
      var serviceUri = ServiceBusEnvironment.CreateServiceUri("https", serviceNamespace, String.Format("{0}/publishers/{1}", hubName, publisherName))
          .ToString()
          .Trim('/');
      return SharedAccessSignatureTokenProvider.GetSharedAccessSignature(senderKeyName, senderKey, serviceUri, tokenTimeToLive);
    }

  }
}
