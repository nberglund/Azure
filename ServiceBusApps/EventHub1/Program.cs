using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace EventHub1
{
  class Program
  {
    static EventHubClient eventHubClient;

    static void Main(string[] args)
    {

      var eventHubName = "nielsderivcoeventhub";
      var connString = GetEventHubConnectionString();

      Receiver r = new Receiver(eventHubName, connString);
      r.MessageProcessingWithPartitionDistribution();



      eventHubClient = EventHubClient.CreateFromConnectionString(connString, eventHubName);

      //SendMessage().Wait();

      var message = Guid.NewGuid().ToString();
      Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), message);
      eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));


      Console.WriteLine("We are done");
      Console.ReadLine();


    }

    private static async Task SendMessage()
    {
      var bet = new Wager
      {
        moduleid = 135,
        clientid = 5,
        userid = 3456,
        usertransnumber = 12,
        sessionid = 2456,
        sessionserverid = 5001,
        totalwager = 10,
        totalpayout = 0,
        gametimeutc = DateTime.UtcNow,
        operatorid = 5000,
        routerid = 345,
        socketid = 1234567
      };

      string json = JsonConvert.SerializeObject(bet);

      await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(json)));
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
